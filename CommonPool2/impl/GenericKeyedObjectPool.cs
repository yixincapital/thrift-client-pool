using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
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

        public void Clear(K key)
        {
            throw new System.NotImplementedException();
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
                        }
                    }
                }
            }
            catch (Exception)
            {
                
                throw;
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

        private void ClearOldest()
        {
            throw new NotImplementedException();
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
            throw new System.NotImplementedException();
        }

        public void InvalidateObject(K key, T obj)
        {
            throw new System.NotImplementedException();
        }
        
        public void AddObject(K key)
        {
            throw new System.NotImplementedException();
        }

        public int GetNumIdle(K key)
        {
            throw new System.NotImplementedException();
        }

        public int GetNumActive(K key)
        {
            throw new System.NotImplementedException();
        }

        public override int GetNumIdle()
        {
            throw new System.NotImplementedException();
        }

        public override int GetNumActive()
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }

          /**
     * Maintains information on the per key queue for a given key.
     */
    public class ObjectDeque<S> {

        private readonly LinkedBlockingDeque<IPooledObject<S>> idleObjects;
      
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