namespace CommonPool2
{
    /// <summary>
    /// A keyed pool maintains a pool of instances for each key value.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    public interface IKeyedObjectPool<K,V>
    {
        /// <summary>
        /// Obtains an instance from this pool for the specified key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        V BorrowObject(K key);

        /// <summary>
        /// Return an instance to the pool. By contract, obj must  have been obtained using
        /// borrowObject or a related method as defined in an implementation or sub-interface 
        /// using a key equivalent to the one used to borrow the instance in the first place.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        void ReturnObject(K key, V obj);

        /// <summary>
        /// Invalidates an object from the pool.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        void InvalidateObject(K key, V obj);

        /// <summary>
        /// Create an object using the  KeyedPooledObjectFactory or
        /// other implementation dependent mechanism, passivate it, and then place it
        /// in the idle object pool. addObject is useful for "pre-loading" a pool with idle objects 
        /// </summary>
        /// <param name="key"></param>
        void AddObject(K key);

        /// <summary>
        /// turns the number of instances corresponding to the given
        /// key currently idle in this pool. Returns a negative value if
        /// this information is not available.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        int GetNumIdle(K key);

        /// <summary>
        /// Returns the number of instances currently borrowed from but not yet
        /// returned to the pool corresponding to the given  key
        /// Returns a negative value if this information is not available.
        /// </summary>
        /// <param name="key">the number of instances currently borrowed from but not yet returned to the pool corresponding to the given key</param>
        /// <returns></returns>
        int GetNumActive(K key);

        /// <summary>
        /// Returns the total number of instances currently idle in this pool.
        /// </summary>
        /// <returns></returns>
        int GetNumIdle();

        /// <summary>
        /// Returns the total number of instances current borrowed from this pool but
        /// not yet returned. Returns a negative value if this information is not available
        /// </summary>
        /// <returns></returns>
        int GetNumActive();

        /// <summary>
        /// Clears the pool, removing all pooled instances (optional operation).
        /// </summary>
        void Clear();

        /// <summary>
        ///  Clears the specified pool, removing all pooled instances corresponding to
        /// the given key 
        /// </summary>
        /// <param name="key"></param>
        void Clear(K key);

        /// <summary>
        /// Close this pool, and free any resources associated with it.
        /// </summary>
        void Close();
    }
}