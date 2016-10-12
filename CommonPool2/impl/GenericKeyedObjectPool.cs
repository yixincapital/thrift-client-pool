using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using CommonPool2;
namespace CommonPool2.impl
{
    public class GenericKeyedObjectPool<K, T> : BaseGenericObjectPool<T>, IKeyedObjectPool<K,T>
    {
        private  int maxIdlePerKey = GenericKeyedObjectPoolConfig.DEFAULT_MAX_IDLE_PER_KEY;
        private  int minIdlePerKey =GenericKeyedObjectPoolConfig.DEFAULT_MIN_IDLE_PER_KEY;
        private  int maxTotalPerKey =GenericKeyedObjectPoolConfig.DEFAULT_MAX_TOTAL_PER_KEY;
        private int numTotal = 0;
        private ReaderWriterLockSlim keyLock = new ReaderWriterLockSlim();
        private IKeyedPooledObjectFactory<K, T> factory;
        private ConcurrentDictionary<K,ObjectDeque<T>> poolMap =new ConcurrentDictionary<K, ObjectDeque<T>>(); 
        private  List<K> poolKeyList = new List<K>();
        public GenericKeyedObjectPool(IKeyedPooledObjectFactory<K, T> factory, GenericKeyedObjectPoolConfig config)
            : base(config)
        { 
            if(factory==null)
                throw new Exception("factory may not be null");
            this.factory = factory;

            SetConfig(config);
        }

        private void SetConfig(GenericKeyedObjectPoolConfig conf)
        {
            setLifo(conf.getLifo());
            SetMaxIdlePerKey(conf.GetMaxIdlePerKey());
            SetMaxTotalPerKey(conf.GetMaxTotalPerKey());
            SetMaxTotal(conf.GetMaxTotal());
            SetMinIdlePerKey(conf.GetMinIdlePerKey());
            SetMaxWaitMillis(conf.getMaxWaitMillis());
            SetBlockWhenExhausted(conf.getBlockWhenExhausted());
            SetTestOnCreate(conf.getTestOnCreate());
            SetTestOnBorrow(conf.getTestOnBorrow());
            SetTestOnReturn(conf.getTestOnReturn());
            SetTestWhileIdle(conf.getTestWhileIdle());
            SetNumTestsPerEvictionRun(conf.getNumTestsPerEvictionRun());
            SetMinEvictableIdleTimeMillis(conf.getMinEvictableIdleTimeMillis());
            SetSoftMinEvictableIdleTimeMillis(
                    conf.getSoftMinEvictableIdleTimeMillis());
            SetTimeBetweenEvictionRunsMillis(
                    conf.getTimeBetweenEvictionRunsMillis());
            SetEvictionPolicyClassName(conf.getEvictionPolicyClassName());
        }

        private void SetMinIdlePerKey(int getMinIdlePerKey)
        {
            this.minIdlePerKey = getMinIdlePerKey;
        }

        private void SetMaxTotalPerKey(int getMaxTotalPerKey)
        {
            this.maxTotalPerKey = getMaxTotalPerKey;
        }

        private void SetMaxIdlePerKey(int getMaxIdlePerKey)
        {
            this.maxIdlePerKey = getMaxIdlePerKey;
        }     

        public override void Close()
        {
            throw new System.NotImplementedException();
        }

        public override void Evict()
        {
            throw new System.NotImplementedException();
        }

        protected override void EnsureMinIdle()
        {
            throw new System.NotImplementedException();
        }

        public T BorrowObject(K key)
        {
            return BorrowObject(key,GetMaxWaitMillis());
        }

        private T BorrowObject(K key, long borrowMaxWaitMillis)
        {
             AssertOpen();
             IPooledObject<T> p = null;

             // Get local copy of current config so it is consistent for entire
             // method execution
             bool blockWhenExhausted = getBlockWhenExhausted();

            bool create;
            long waitTime = DateTime.Now.CurrentTimeMillis();
            ObjectDeque<T> objectDeque = Register(key);
            try
            {
                while (p == null)
                {
                    create = false;
                    if (blockWhenExhausted)
                    {
                        p = objectDeque.GetIdleObjects().RemoveFront();
                        if (p == null)
                        {
                            p = Create(key);
                            if (p != null)
                            {
                                create = true;
                            }
                        }
                        if (p == null)
                        {
                            if (borrowMaxWaitMillis < 0)
                            {
                                p = objectDeque.GetIdleObjects().TakeFront();
                            }
                            else
                            {
                                p = objectDeque.GetIdleObjects().RemoveFront();
                            }
                        }
                        if (p == null)
                        {
                            throw new Exception(
                                "Timeout waiting for idle object");
                        }
                        if (!p.Allocate())
                        {
                            p = null;
                        }
                    }
                    else
                    {
                        p = objectDeque.GetIdleObjects().RemoveFront();
                        if (p == null)
                        {
                            p = Create(key);
                            if (p != null)
                            {
                                create = true;
                            }
                        }
                        if (p == null)
                        {
                            throw new Exception("Pool exhausted");
                        }
                        if (!p.Allocate())
                        {
                            p = null;
                        }
                    }
                    if (p != null)
                    {
                        try
                        {
                            factory.ActivateObject(key, p);
                        }
                        catch (Exception e)
                        {
                            try
                            {
                                Destroy(key, p, true);
                            }
                            catch (Exception)
                            {
                                // ignore  
                            }
                            p = null;
                            if (create)
                            {
                                throw new Exception("Unable to activate object");
                            }
                        }
                        if (p != null && (GetTestOnBorrow() || create && getTestOnCreate()))
                        {
                            bool validate = false;
                            try
                            {
                                validate = factory.ValidateObject(key, p);
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                            if (!validate)
                            {
                                try
                                {
                                    Destroy(key, p, true);
                                    destroyedByBorrowValidationCount++;
                                }
                                catch (Exception)
                                {
                                    // Ignore - validation failure is more important
                                }
                                p = null;
                                if (create)
                                {
                                    throw new Exception("Unable to validate object");
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                Deregister(key); 
            }
            UpdateStatsBorrow(p, DateTime.Now.CurrentTimeMillis() - waitTime);
            return p.GetObject();
        }

        private void Deregister(K key)
        {
            ObjectDeque<T> objectDeque;

            poolMap.TryGetValue(key, out objectDeque);

            long numInterested = objectDeque.DecrementAndGet();
            if (numInterested == 0 && objectDeque.GetCreateCount() == 0)
            {
                // Potential to remove key
                ReaderWriterLockSlim locker =new ReaderWriterLockSlim();
                locker.EnterWriteLock();
                try
                {
                    if (objectDeque.GetCreateCount() == 0 && objectDeque.GetNumInterested() == 0)
                    {
                        // NOTE: Keys must always be removed from both poolMap and
                        //       poolKeyList at the same time while protected by
                        //       keyLock.writeLock()
                        poolMap.TryRemove(key, out objectDeque);
                        poolKeyList.Remove(key);
                    }
                }
                finally
                {
                    locker.ExitWriteLock();
                }
              //  locker.Dispose();               
            }
        }

        /// <summary>
        /// Destroy the wrapped, pooled object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="toDestroy"></param>
        /// <param name="always"></param>
        private bool Destroy(K key, IPooledObject<T> toDestroy, bool always)
        {
            ObjectDeque<T> objectDeque = Register(key);
            try
            {
                bool isIdle = objectDeque.GetIdleObjects().Remove(toDestroy);

                if (isIdle || always)
                {
                    objectDeque.GetAllObjects().TryRemove(new IdentityWrapper<T>(toDestroy.GetObject()), out toDestroy);
                    toDestroy.Invalidate();

                    try
                    {
                        factory.DestroyObject(key, toDestroy);
                    }
                    finally
                    {
                        createdCount--;
                        destroyedCount++;
                        numTotal--;
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                Deregister(key);
            }
        }

        private IPooledObject<T> Create(K key)
        {
            int maxTotalPerKeySave = GetMaxTotalPerKey(); // Per key
            int maxTotal = GetMaxTotal();   // All keys

            // Check against the overall limit
            bool loop = true;

            while (loop)
            {
                int newNumTotal = ++numTotal;
                if (maxTotal > -1 && newNumTotal > maxTotal)
                {
                    --numTotal;
                    if (GetNumIdle() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        ClearOldest();
                    }
                }
                else
                {
                    loop = false;
                }
            }

            ObjectDeque<T> objectDeque;
            poolMap.TryGetValue(key, out objectDeque);
            long newCreateCount = objectDeque.GetCreateCount()+1;

            // Check against the per key limit
            if (maxTotalPerKeySave > -1 && newCreateCount > maxTotalPerKeySave ||
                    newCreateCount > 0x7fffffff)
            {
                --numTotal;
                objectDeque.DecrementAndGet();
                return null;
            }


            IPooledObject<T> p = null;
            try
            {
                p = factory.MakeObject(key);
            }
            catch (Exception e)
            {
                numTotal--;
                objectDeque.DecrementAndGet();
                throw e;
            }

            ++createdCount;
            objectDeque.GetAllObjects().TryAdd(new IdentityWrapper<T>(p.GetObject()), p);
            return p;
        }

        /// <summary>
        /// Clears oldest 15% of objects in pool.  The method sorts the objects into
        /// a TreeMap and then iterates the first 15% for removal.
        /// </summary>
        private void ClearOldest()
        {
            // build sorted map of idle objects
            Dictionary<IPooledObject<T>,K> map =new Dictionary<IPooledObject<T>, K>();

            foreach (var objectDeque in poolMap)
            {
                var queue = objectDeque.Value;
                if (queue != null)
                {
                    LinkedBlockingDeque<IPooledObject<T>> idleObjects =
                        queue.GetIdleObjects();
                    foreach (var idleObject in idleObjects)
                    {
                        // each item into the map using the PooledObject object as the
                        // key. It then gets sorted based on the idle time
                        map.Add(idleObject,objectDeque.Key);
                    }
                }
            }

            // Now iterate created map and kill the first 15% plus one to account 
            // for zero
            int itemsToRemove = (int) ((map.Count*0.15) + 1);
            var iter = map.GetEnumerator();
            do
            {
                var entry = iter.Current;
                K key = entry.Value;
                IPooledObject<T> p = entry.Key;
                bool destroyed = true;
                try
                {
                    destroyed = Destroy(key, p, false);
                }
                catch (Exception e)
                {                    
                    throw e;
                }

                if (destroyed)
                {
                    itemsToRemove--;
                }
            } while (iter.MoveNext());

        }

        private int GetMaxTotalPerKey()
        {
            return maxTotalPerKey;
        }

        private ObjectDeque<T> Register(K key)
        {
            ObjectDeque<T> objectDeque = null;
            try
            {
                keyLock.EnterReadLock();
                poolMap.TryGetValue(key, out objectDeque);
                if (objectDeque == null)
                {
                    keyLock.ExitReadLock();
                    // Upgrade to write lock
                    keyLock.EnterWriteLock();
                    objectDeque = new ObjectDeque<T>(true);
                    objectDeque.InCrementAndGet();
                    poolMap.TryAdd(key, objectDeque);
                    poolKeyList.Add(key);
                }
                else
                {
                    objectDeque.InCrementAndGet();
                }
            }
            finally
            {
                keyLock.ExitWriteLock();
            }
            return objectDeque;
        }

        public void ReturnObject(K key, T obj)
        {
            ObjectDeque<T> objectDeque = poolMap[key];

            IPooledObject<T> p;
            objectDeque.GetAllObjects().TryGetValue(new IdentityWrapper<T>(obj), out p);
            if (p == null)
            {
                throw new IllegalStateException("Returned object not currently part of this pool");
            }

            lock (p)
            {
                PooledObjectState state = p.GetState();
                if (state != PooledObjectState.Allocated)
                {
                    throw new IllegalStateException(
                        "Object has already been returned to this pool or is invalid");
                }
                else
                {
                    p.MarkReturning(); // Keep from being marked abandoned
                }
            }

            long activeTime = p.GetActiveTimeMillis();

            if (GetTestOnReturn())
            {
                if (!factory.ValidateObject(key, p))
                {
                    try
                    {
                        Destroy(key, p, true);
                    }
                    catch (Exception e)
                    {
                        
                        throw e;
                    }
                    if (objectDeque.idleObjects.HasTakeWaiters())
                    {
                        try
                        {
                            AddObject(key);
                        }
                        catch (Exception e)
                        {
                            
                            throw e;
                        }
                    }
                    UpdateStatsReturn(activeTime);
                    return;
                }
            }
            try
            {
                factory.PassivateObject(key,p);
            }
            catch (Exception e)
            {                
                throw e;
            }
            if (objectDeque.idleObjects.HasTakeWaiters())
            {
                try
                {
                    AddObject(key);
                }
                catch (Exception e)
                {
                    
                    throw e;
                }
                UpdateStatsReturn(activeTime);
                return;
            }

            if (!p.Deallocate())
            {
                throw new IllegalStateException(
                  "Object has already been returned to this pool");
            }

            int maxIdle = GetMaxTotalPerKey();
            LinkedBlockingDeque<IPooledObject<T>> ideleObjects = objectDeque.GetIdleObjects();
            if (IsClosed() || maxIdle > -1 && maxIdle <= ideleObjects.Count)
            {
                try
                {
                    Destroy(key, p, true);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            else
            {
                ideleObjects.AddFront(p);
                if (IsClosed())
                {
                    // Pool closed while object was being added to idle objects.
                    // Make sure the resturned object is destroyed rather than left
                    // in the idle object pool (which would effectively be a leak)
                    Clear(key);
                }
            }
            if (HasBorrowWaiters())
            {
                //TODO   未来实现resueCapacity功能
            }
            UpdateStatsReturn(activeTime);
        }

        public void InvalidateObject(K key, T obj)
        {
            ObjectDeque<T> objectDeque;
            poolMap.TryGetValue(key,out objectDeque);
            IPooledObject<T> p = null;
            if (objectDeque != null) objectDeque.GetAllObjects().TryGetValue(new IdentityWrapper<T>(obj), out p);
            if(p==null)
                throw new IllegalStateException("Object not currently part of this pool");
            lock (p)
            {
                if (p.GetState() != PooledObjectState.Invalid)
                {
                    Destroy(key, p, true);
                }
            }
            if (objectDeque.idleObjects.HasTakeWaiters())
            {
                AddObject(key);
            }
        }
        
        public void AddObject(K key)
        {
            AssertOpen();
            Register(key);
            try
            {
                IPooledObject<T> p = Create(key);
                AddIdleObject(key, p);
            }
            finally
            {
                Deregister(key);
            }
        }

        public int GetNumIdle(K key)
        {
            ObjectDeque<T> objectDeque;
            poolMap.TryGetValue(key, out objectDeque);
            return objectDeque != null ? objectDeque.GetIdleObjects().Count : 1;
        }

        public int GetNumActive(K key)
        {
            ObjectDeque<T> objectDeque;
            poolMap.TryGetValue(key, out objectDeque);
            if (objectDeque != null)
            {
                return objectDeque.GetAllObjects().Count - objectDeque.GetIdleObjects().Count;
            }
            else
            {
                return 0;
            }
        }

        public override int GetNumIdle()
        {
            var iter = poolMap.Values.GetEnumerator();
            int result = 0;

            do
            {
                result += iter.Current.GetIdleObjects().Count;
            } while (iter.MoveNext());
            return result;
        }

        public override int GetNumActive()
        {
            return numTotal - GetNumIdle();
        }

        public void Clear()
        {
            var iter = poolMap.Keys.GetEnumerator();

            do
            {
                Clear(iter.Current);

            } while (iter.MoveNext());
        }
        
          /**
     * Clears the specified sub-pool, removing all pooled instances
     * corresponding to the given <code>key</code>. Exceptions encountered
     * destroying idle instances are swallowed but notified via a
     * {@link SwallowedExceptionListener}.
     *
     * @param key the key to clear
     */

    public void Clear(K key) {

        ObjectDeque<T> objectDeque = Register(key);

        try {
            LinkedBlockingDeque<IPooledObject<T>> idleObjects =
                    objectDeque.GetIdleObjects();

            IPooledObject<T> p = idleObjects.Poll();

            while (p != null) {
                try {
                    Destroy(key, p, true);
                } catch (Exception e)
                {
                    throw e;
                }
                p = idleObjects.Poll();
            }
        } finally {
            Deregister(key);
        }
    }

        /// <summary>
        /// Checks to see if there are any threads currently waiting to borrow
        /// objects but are blocked waiting for more objects to become available.
        /// </summary>
        /// <returns>true if there is at least one thread waiting otherwise false</returns>
        private bool HasBorrowWaiters()
        {
            foreach (var k in poolMap.Keys)
            {
                ObjectDeque<T> deque;
                poolMap.TryGetValue(k, out deque);
                if (deque != null)
                {
                    LinkedBlockingDeque<IPooledObject<T>> pool = deque.GetIdleObjects();
                    if (pool.HasTakeWaiters())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void AddIdleObject(K key, IPooledObject<T> p)
        {
            if (p != null)
            {
                factory.PassivateObject(key,p);
              
                ObjectDeque<T> deque;
                poolMap.TryGetValue(key, out deque);
                if (deque != null)
                {
                    LinkedBlockingDeque<IPooledObject<T>> idleObjects= deque.GetIdleObjects();
                    idleObjects.AddFront(p);
                }
            }
        }
          /**
     * Maintains information on the per key queue for a given key.
     */
    public class ObjectDeque<S> {

        public  LinkedBlockingDeque<IPooledObject<S>> idleObjects;
      
        /*
         * Number of instances created - number destroyed.
         * Invariant: createCount <= maxTotalPerKey
         */
        private int createCount = 0;

        /*
         * The map is keyed on pooled instances, wrapped to ensure that
         * they work properly as keys.  
         */
        private  ConcurrentDictionary<IdentityWrapper<S>, IPooledObject<S>> allObjects =
                new ConcurrentDictionary<IdentityWrapper<S>, IPooledObject<S>>();
        
        /*
         * Number of threads with registered interest in this key.
         * register(K) increments this counter and deRegister(K) decrements it.
         * Invariant: empty keyed pool will not be dropped unless numInterested
         *            is 0.
         */
        private long numInterested =0;

        /**
         * Create a new ObjecDeque with the given fairness policy.
         * @param fairness true means client threads waiting to borrow / return instances
         * will be served as if waiting in a FIFO queue.
         */
        public ObjectDeque(bool fairness) {
            idleObjects = new LinkedBlockingDeque<IPooledObject<S>>();
        }

        /**
         * Obtain the idle objects for the current key.
         *
         * @return The idle objects
         */
        public LinkedBlockingDeque<IPooledObject<S>> GetIdleObjects() {
            return idleObjects;
        }

        /**
         * Obtain the count of the number of objects created for the current
         * key.
         *
         * @return The number of objects created for this key
         */
        public int GetCreateCount() {
            return createCount;
        }

        /**
         * Obtain the number of threads with an interest registered in this key.
         *
         * @return The number of threads with a registered interest in this key
         */
        public long GetNumInterested() {
            return numInterested;
        }

        public long InCrementAndGet()
        {
            numInterested++;
            return numInterested;
        }

        public long DecrementAndGet()
        {
            --numInterested;
            return numInterested;
        }
        /**
         * Obtain all the objects for the current key.
         *
         * @return All the objects
         */
        public ConcurrentDictionary<IdentityWrapper<S>, IPooledObject<S>> GetAllObjects() {
            return allObjects;
        }
       
    }
    }
}