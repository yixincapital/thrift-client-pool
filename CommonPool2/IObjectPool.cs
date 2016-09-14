namespace CommonPool2
{
    public interface IObjectPool<T>
    {
        /// <summary>
        /// Obtains an instance from this pool.
        /// </summary>
        /// <returns>an instance from this pool.</returns>
        T BorrowObject();

        /// <summary>
        /// Return an instance to the pool. By contract ,obj must
        /// have been obtained using  borrowObject() or
        /// a related method as defined in an implementation or sub-interface.
        /// </summary>
        /// <param name="obj"></param>
        void ReturnObject(T obj);

        /// <summary>
        /// Invalidates an object from the pool.
        /// </summary>
        /// <param name="obj"></param>
        void InvalidateObject(T obj);

        /// <summary>
        /// Create an object using the  PooledObjectFactory or other 
        /// implementation dependent mechanism, passivate it, and then place it in
        /// the idle object pool. <code>addObject</code> is useful for "pre-loading"
        /// a pool with idle objects.
        /// </summary>
        void AddObject();

        /// <summary>
        /// Return the number of instances currently idle in this pool. This may be
        /// onsidered an approximation of the number of objects that can be
        /// #borrowObject borrowed without creating any new instances
        /// Returns a negative value if this information is not available.
        /// </summary>
        /// <returns>the number of instances currently idle in this pool.</returns>
        int GetNumIdle();

        /// <summary>
        /// Return the number of instances currently borrowed from this pool. Returns
        /// a negative value if this information is not available.
        /// </summary>
        /// <returns>the number of instances currently borrowed from this pool.</returns>
        int GetNumActive();

        /// <summary>
        /// Clears any objects sitting idle in the pool, releasing any associated
        /// resources (optional operation). Idle objects cleared must be PooledObjectFactory#destroyObject
        /// </summary>
        void Clear();

        /// <summary>
        ///  Close this pool, and free any resources associated with it.
        /// </summary>
        void Close();
    }
}