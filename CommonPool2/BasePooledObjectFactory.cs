namespace CommonPool2
{
    public abstract class BasePooledObjectFactory<T>:IPooledObjectFactory<T>
    {
        /// <summary>
        /// Creates an object instance, to be wrapped in a  PooledObject
        /// </summary>
        /// <returns></returns>
        public abstract T Create();

        /// <summary>
        /// Wrap the provided instance with an implementation of PooledObject
        /// </summary>
        /// <param name="obj">The provided instance, wrapped by a  PooledObject</param>
        /// <returns></returns>
        public abstract IPooledObject<T> Wrap(T obj);

        public IPooledObject<T> MakeObject()
        {
            return Wrap(Create());
        }

        public void DestroyObject(IPooledObject<T> p)
        {
            
        }
        /// <summary>
        /// always return true
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool ValidateObject(IPooledObject<T> p)
        {
            return true;
        }

        public  void ActivateObject(IPooledObject<T> p)
        { }
        public abstract void PassivateObject(IPooledObject<T> p);
    }
}