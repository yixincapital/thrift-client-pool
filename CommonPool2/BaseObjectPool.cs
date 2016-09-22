namespace CommonPool2
{
    /// <summary>
    /// BaseObjectPool
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseObjectPool<T> : IObjectPool<T>
    {
        private volatile bool closed = false;
        public abstract T BorrowObject();

        public abstract void ReturnObject(T obj);

        public abstract void InvalidateObject(T obj);

        public abstract void AddObject();

        public int GetNumIdle()
        {
            return -1;
        }

        public int GetNumActive()
        {
            return -1;
        }

        public abstract void Clear();

        public void Close()
        {
            closed = true;
        }

        public bool IsClosed()
        {
            return closed;
        }

        protected void AssertOpen()
        {
            if (IsClosed())
            {
                throw new IllegalStateException("Pool not open");
            }
        }
    }
}