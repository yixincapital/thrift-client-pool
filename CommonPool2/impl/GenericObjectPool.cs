using System;
using System.Collections.Concurrent;
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

        private void RemoveAbandoned(AbandonedConfig ac)
        {
            throw new NotImplementedException();
        }

        public void ReturnObject(T obj)
        {
            throw new System.NotImplementedException();
        }

        public void InvalidateObject(T obj)
        {
            throw new System.NotImplementedException();
        }

        public void AddObject()
        {
            throw new System.NotImplementedException();
        }

        public override int GetNumIdle()
        {
            throw new System.NotImplementedException();
        }

        public int GetNumActive()
        {
            throw new System.NotImplementedException();
        }
    }
}