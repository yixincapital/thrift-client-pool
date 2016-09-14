using System;

namespace CommonPool2.impl
{
    public class GenericObjectPool<T> : BaseGenericObjectPool<T>,IObjectPool<T>
    {
         private  IPooledObjectFactory<T> factory;
         private volatile int maxIdle = GenericObjectPoolConfig.DEFAULT_MAX_IDLE;
         private volatile int minIdle = GenericObjectPoolConfig.DEFAULT_MIN_IDLE;
         private  LinkedBlockingDeque<IPooledObject<T>> idleObjects;
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
            throw new System.NotImplementedException();
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