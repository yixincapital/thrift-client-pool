namespace CommonPool2
{
    public interface IKeyedPooledObjectFactory<K,V>
    {
        /// <summary>
        ///  Create an instance that can be served by the pool and wrap it in a
        /// PooledObject to be managed by the pool.
        /// </summary>
        /// <param name="key">the key used when constructing the object</param>
        /// <returns>PoolObject wrapping an instance that can be served by the pool.</returns>
        IPooledObject<V> MakeObject(K key);

        /// <summary>
        /// Destroy an instance no longer needed by the pool.
        /// </summary>
        /// <param name="key">the key used when selecting the instance</param>
        /// <param name="p">PoolObject wrapping the instance to be destroyed</param>
        void DestroyObject(K key, IPooledObject<V> p);

        /// <summary>
        /// nsures that the instance is safe to be returned by the pool.
        /// </summary>
        /// <param name="key">the key used when selecting the object</param>
        /// <param name="p">PooledObject wrapping the instance to be validated</param>
        /// <returns></returns>
        bool ValidateObject(K key, IPooledObject<V> p);

        /// <summary>
        /// Reinitialize an instance to be returned by the pool.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="p"></param>
        void ActivateObject(K key, IPooledObject<V> p);

        /// <summary>
        /// Uninitialize an instance to be returned to the idle object pool.
        /// </summary>
        /// <param name="key">the key used when selecting the object</param>
        /// <param name="p">PooledObject wrapping the instance to be passivated</param>
        void PassivateObject(K key, IPooledObject<V> p);
    }
}