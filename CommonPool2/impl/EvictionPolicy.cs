namespace CommonPool2.impl
{
    public interface EvictionPolicy<T>
    {
        /// <summary>
        /// This method is called to test if an idle object in the pool should be evicted or not
        /// </summary>
        /// <param name="config"></param>
        /// <param name="underTest"></param>
        /// <param name="idleCount"></param>
        /// <returns></returns>
        bool Evict(EvictionConfig config, IPooledObject<T> underTest,
          int idleCount); 
    }
}