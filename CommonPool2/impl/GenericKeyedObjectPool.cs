namespace CommonPool2.impl
{
    public class GenericKeyedObjectPool<K, T> : BaseGenericObjectPool<T>, IKeyedObjectPool<K,T>
    {
        public GenericKeyedObjectPool(BaseObjectPoolConfig config) : base(config)
        {
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
            throw new System.NotImplementedException();
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

        public int GetNumActive()
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }
    }
}