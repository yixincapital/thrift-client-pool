namespace CommonPool2.impl
{
    public class EvictionConfig
    {
        private readonly long idleEvictTime;
        private readonly long idleSoftEvictTime;
        private readonly int minIdle;

        public EvictionConfig(long poolIdleEvictTime, long poolIdleSoftEvictTime,
            int minIdle)
        {
            if (poolIdleEvictTime > 0)
            {
                idleEvictTime = poolIdleEvictTime;
            }
            else
            {
                idleEvictTime = 0x7fffffffffffffffL;
            }
            if (poolIdleSoftEvictTime > 0)
            {
                idleSoftEvictTime = poolIdleSoftEvictTime;
            }
            else
            {
                idleSoftEvictTime = 0x7fffffffffffffffL;
            }
            this.minIdle = minIdle;
        }

        /// <summary>
        /// Obtain the {@code idleEvictTime} for this eviction configuration instance
        /// </summary>
        /// <returns> The {@code idleEvictTime} in milliseconds</returns>
        public long GetIdleEvictTime()
        {
            return idleEvictTime;
        }

        /// <summary>
        /// Obtain the idleSoftEvictTime for this eviction configuration
        /// </summary>
        /// <returns>The (@code idleSoftEvictTime} in milliseconds</returns>
        public long GetIdleSoftEvictTime()
        {
            return idleSoftEvictTime;
        }

        /// <summary>
        /// btain theminIdle for this eviction configuration instance.
        /// </summary>
        /// <returns>minIdle</returns>
        public int GetMinIdle()
        {
            return minIdle;
        }
    }
}