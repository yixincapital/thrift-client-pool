using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CommonPool2.impl
{
    public class GenericObjectPool<T> : BaseGenericObjectPool<T>,IObjectPool<T>
    {
        private readonly IPooledObjectFactory<T> factory;
         private volatile int maxIdle = GenericObjectPoolConfig.DEFAULT_MAX_IDLE;
         private volatile int minIdle = GenericObjectPoolConfig.DEFAULT_MIN_IDLE;
        private int createCount = 0;
         private  LinkedBlockingDeque<IPooledObject<T>> idleObjects;
        private AbandonedConfig abandonedConfig;        
        private readonly ConcurrentDictionary<IdentityWrapper<T>, IPooledObject<T>> _allObjects =
        new ConcurrentDictionary<IdentityWrapper<T>, IPooledObject<T>>();
        public GenericObjectPool(IPooledObjectFactory<T> factory,
            GenericObjectPoolConfig config):base(config) 
        {

            if (factory == null)
            {
              
                throw new Exception("factory may not be null");
            }
            this.factory = factory;

            idleObjects = new LinkedBlockingDeque<IPooledObject<T>>();

            SetConfig(config);

           // TODO startEvictor(getTimeBetweenEvictionRunsMillis());
        }

        private void SetConfig(GenericObjectPoolConfig conf)
        {
           // SetLifo(conf.getLifo());
            SetMaxIdle(conf.GetMaxIdle());
            SetMinIdle(conf.GetMinIdle());
            SetMaxTotal(conf.GetMaxTotal());
            SetMaxWaitMillis(conf.getMaxWaitMillis());
            SetBlockWhenExhausted(conf.getBlockWhenExhausted());
            SetTestOnCreate(conf.getTestOnCreate());
            SetTestOnBorrow(conf.getTestOnBorrow());
            SetTestOnReturn(conf.getTestOnReturn());
            SetTestWhileIdle(conf.getTestWhileIdle());
            SetNumTestsPerEvictionRun(conf.getNumTestsPerEvictionRun());
            SetMinEvictableIdleTimeMillis(conf.getMinEvictableIdleTimeMillis());
            SetTimeBetweenEvictionRunsMillis(
                    conf.getTimeBetweenEvictionRunsMillis());
            SetSoftMinEvictableIdleTimeMillis(
                    conf.getSoftMinEvictableIdleTimeMillis());
            SetEvictionPolicyClassName(conf.getEvictionPolicyClassName());
        }

        
        private void SetMinIdle(int getMinIdle)
        {
            this.minIdle = minIdle;
        }

        private void SetMaxIdle(int getMaxIdle)
        {
            this.maxIdle = maxIdle;
        }

        private void SetLifo(bool getLifo)
        {
            
        }

        public void Clear()
        {
            idleObjects.Clear();
        }

        /// <summary>
        /// Closes the pool. 
        /// </summary>
        public override void Close()
        {
            if (IsClosed())
            {
                return;
            }
            lock (closeLock)
            {
                if (IsClosed())
                {
                    return;
                }
                closed = true;
            }
        }

        public override void Evict()
        {
            throw new System.NotImplementedException();
        }

        protected override void EnsureMinIdle()
        {
            throw new System.NotImplementedException();
        }

        public T BorrowObject()
        {
            return BorrowObject(GetMaxWaitMillis());
        }
         public T BorrowObject(long borrowMaxWaitMillis)  {
        AssertOpen();

        AbandonedConfig ac = this.abandonedConfig;
        if (ac != null && ac.getRemoveAbandonedOnBorrow() &&
                (GetNumIdle() < 2) &&
                (GetNumActive() > GetMaxTotal() - 3) ) {
            RemoveAbandoned(ac);
        }

        IPooledObject<T> p = null;

        // Get local copy of current config so it is consistent for entire
        // method execution
        bool blockWhenExhausted = getBlockWhenExhausted();

        bool create;
        long waitTime = DateTime.Now.Millisecond;

        while (p == null) {
            create = false;
            if (blockWhenExhausted) {
                p = idleObjects.RemoveFront();
                if (p == null) {
                    p = Create();
                    if (p != null) {
                        create = true;
                    }
                }
                if (p == null) {
                    if (borrowMaxWaitMillis < 0) {
                        p = idleObjects.First();
                    } else {
                        p = idleObjects.RemoveFront();
                    }
                }
                if (p == null) {
                    throw new Exception(
                            "Timeout waiting for idle object");
                }
                if (!p.Allocate()) {
                    p = null;
                }
            } else {
                p = idleObjects.RemoveFront();
                if (p == null) {
                    p = Create();
                    if (p != null) {
                        create = true;
                    }
                }
                if (p == null) {
                    throw new Exception("Pool exhausted");
                }
                if (!p.Allocate()) {
                    p = null;
                }
            }

            if (p != null) {
                try {
                    factory.ActivateObject(p);
                } catch (Exception e) {
                    try {
                        Destroy(p);
                    } catch (Exception e1) {
                        // Ignore - activation failure is more important
                    }
                    p = null;
                    if (create) {
                     
                        throw new Exception("Unable to activate object");
                    }
                }
                if (p != null && (GetTestOnBorrow() || create && getTestOnCreate())) {
                    bool validate = false;
                   // Throwable validationThrowable = null;
                    try {
                        validate = factory.ValidateObject(p);
                    } catch (Exception t)
                    {
                        throw t;
                        //   validationThrowable = t;
                    }
                    if (!validate) {
                        try {
                            Destroy(p);
                           
                        } catch (Exception e) {
                            // Ignore - validation failure is more important
                        }
                        p = null;
                        if (create) {
                              throw new Exception("Unable to activate object");
                        }
                    }
                }
            }
        }

        UpdateStatsBorrow(p, DateTime.Now.Millisecond - waitTime);

        return p.GetObject();
    }

        /// <summary>
         /// Updates statistics after an object is borrowed from the pool.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="waitTime"></param>
        private void UpdateStatsBorrow(IPooledObject<T> p, long waitTime)
        {
            _borrowedCount++;
            idleTimes.Add(p.GetIdleTimeMillis());
            waitTimes.Add(waitTime);
        }

        /// <summary>
         /// Attempts to create a new wrapped pooled object.
        /// </summary>
        /// <returns></returns>
        private IPooledObject<T> Create()
        {
            int localMaxTotal = GetMaxTotal();
            long newCreateCount = createCount+1;
            if(localMaxTotal >-1 && newCreateCount>localMaxTotal || newCreateCount>0x7fffffff)
            {
                createCount--;
                return null;
            }
            
            IPooledObject<T> p;
            try
            {
                p = factory.MakeObject();
            }
            catch (Exception e)
            {
                createCount--;
                throw e;
            }
            createCount++;
            _allObjects.TryAdd(new IdentityWrapper<T>(p.GetObject()), p);
            return p;
        }

        private void Destroy(IPooledObject<T> toDestroy)
        {
            IPooledObject<T> obj;
            toDestroy.Invalidate();
            idleObjects.Remove(toDestroy);
            _allObjects.TryRemove(new IdentityWrapper<T>(toDestroy.GetObject()), out obj);
            try
            {
                factory.DestroyObject(toDestroy);
            }
            finally
            {
                createCount++;
            }
        }

        /// <summary>
        /// Recover abandoned objects which have been checked out but
        /// not used since longer than the removeAbandonedTimeout.
        /// </summary>
        /// <param name="ac"></param>
        private void RemoveAbandoned(AbandonedConfig ac)
        {
            long now = DateTime.Now.Millisecond;
            long timeout = now - ac.getRemoveAbandonedTimeout()*1000L;
            List<IPooledObject<T>> remove = new List<IPooledObject<T>>();
            IEnumerator<IPooledObject<T>> it = _allObjects.Values.GetEnumerator();
            do
            {
                IPooledObject<T> pooledObject = it.Current;
                lock (pooledObject)
                {
                    if (pooledObject.GetState() == PooledObjectState.Allocated &&
                        pooledObject.GetLastUsedTime() <= timeout)
                    {
                        pooledObject.MarkAbandoned();
                        remove.Add(pooledObject);
                    }
                }
            } while (it.MoveNext());

            // Now remove the abandoned objects
            IEnumerator<IPooledObject<T>> itr =remove.GetEnumerator();

            do
            {
                IPooledObject<T> pooledObject = itr.Current;
                try
                {
                    InvalidateObject(pooledObject.GetObject());
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            } while (itr.MoveNext());
        }

        public void ReturnObject(T obj)
        {
            IPooledObject<T> p;
            _allObjects.TryGetValue(new IdentityWrapper<T>(obj), out p);

            if (p == null)
            {
                return;
            }

            lock (p)
            {
                var state = p.GetState();
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
                if (!factory.ValidateObject(p))
                {
                    try
                    {
                        Destroy(p);
                    }
                    catch (Exception)
                    {                        
                        throw;
                    }
                    try
                    {
                        EnsureIdle(1, false);
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                    UpdateStatsReturn(activeTime);
                }
            }
            try
            {
                factory.PassivateObject(p);
            }
            catch (Exception e1)
            {
               
                try
                {
                    Destroy(p);
                }
                catch (Exception e)
                {
                    throw e;
                }
                try
                {
                    EnsureIdle(1, false);
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
                        "Object has already been returned to this pool or is invalid");
            }

            int maxIdleSave = GetMaxIdle();
            if (IsClosed() || maxIdleSave > -1 && maxIdleSave <= idleObjects.Count)
            {
                try
                {
                    Destroy(p);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            else
            {               
                idleObjects.AddBack(p);             
                if (IsClosed())
                {
                    // Pool closed while object was being added to idle objects.
                    // Make sure the returned object is destroyed rather than left
                    // in the idle object pool (which would effectively be a leak)
                    Clear();
                }
            }
            UpdateStatsReturn(activeTime);
        }
         
        public int GetMaxIdle()
        {
            return maxIdle;
        }
        public void InvalidateObject(T obj)
        {

            IPooledObject<T> p;
            _allObjects.TryGetValue(new IdentityWrapper<T>(obj), out p);
            if (p == null)
            {
                throw new Exception(
                      "Invalidated object not currently part of this pool");
            }
            lock (p)
            {
                if (p.GetState() != PooledObjectState.Invalid)
                {
                    Destroy(p);
                }
            }

            EnsureIdle(1, false);
        }

        private void EnsureIdle(int idleCount, bool always)
        {
            if (idleCount < 1 || IsClosed() || (!always && !idleObjects.HasTakeWaiters()))
            {
                return;
            }

            while (idleObjects.Count<idleCount)
            {
                IPooledObject<T> p = Create();
                if (p == null)
                {
                    // Can't create objects, no reason to think another call to
                    // create will work. Give up.
                    break;
                }
                idleObjects.AddBack(p); // default add the instance to the end of deque
            }
            if (IsClosed())
            {
                // Pool closed while object was being added to idle objects.
                // Make sure the returned object is destroyed rather than left
                // in the idle object pool (which would effectively be a leak)
                Clear();
            }
        }

        /// <summary>
        /// Create an object, and place it into the pool. addObject() is useful for
        /// "pre-loading" a pool with idle objects.
        /// </summary>
        public void AddObject()
        {
            AssertOpen();
            if (factory == null)
            {
                throw new IllegalStateException(
                        "Cannot add objects without a factory.");
            }
            IPooledObject<T> p = Create();
            AddIdleObject(p);
        }
        
        /// <summary>
        /// add the provided wrapped pooled object to the set of idle objects for
        /// this pool. The object must already be part of the pool.  If  p
        ///  is null, this is a no-op (no exception, but no impact on the pool).
        /// </summary>
        /// <param name="pooledObject"></param>
        private void AddIdleObject(IPooledObject<T> p)
        {
            if (p != null)
            {
                factory.PassivateObject(p);
             
                idleObjects.AddBack(p);          
            }
        }

        public override int GetNumIdle()
        {
            return idleObjects.Count;
        }

        public override int GetNumActive()
        {
            return _allObjects.Count - idleObjects.Count;
        }
    }
}