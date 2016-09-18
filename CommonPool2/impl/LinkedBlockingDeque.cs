using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Serialization;


namespace CommonPool2.impl
{
    public class LinkedBlockingDeque<E> : Deque<E>
    {
        /// <summary>
        /// Returns true if there are threads waiting to take instances from this deque.
        /// TODO  will find some like way in C#
        /// </summary>
        /// <returns></returns>
        public bool HasTakeWaiters()
        {
            return false;
        }
    }
}