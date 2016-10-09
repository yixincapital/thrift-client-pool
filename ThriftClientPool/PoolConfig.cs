using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThriftClientPool
{
    public class PoolConfig
    {
        private int timeout = 0;
        public bool failover = false;

        /// <summary>
        /// get default connection socket timeout(default 0, means not timeout)
        /// </summary>
        /// <returns></returns>
        public int GetTimeout()
        {
            return timeout;
        }

        /// <summary>
        /// set default connection timeout
        /// </summary>
        /// <param name="timeout">timeout millis</param>
        public void SetTimeout(int timeout)
        {
            this.timeout = timeout;
        }

        /// <summary>
        /// get connection to next service if one service fail(default is false)
        /// </summary>
        /// <returns></returns>
        public bool IsFailover()
        {
            return failover;
        }

        /// <summary>
        /// set connection to next service if one service fail
        /// </summary>
        /// <param name="failover"></param>
        public void SetFailover(bool failover)
        {
            this.failover = failover;
        }
    }
}
