namespace CommonPool2.impl
{
    public class GenericObjectPool<T> : BaseGenericObjectPool<T>,IObjectPool<T>
    {
     
        public GenericObjectPool(BaseObjectPoolConfig config) : base(config)
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