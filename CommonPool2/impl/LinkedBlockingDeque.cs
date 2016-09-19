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
        /// <summary>
        /// Unlinks the first element in the queue, waiting until there is an element
        /// to unlink if the queue is empty.
        /// TODO will do the work later ,now use removefront 
        /// </summary>
        /// <returns></returns>
        public E TakeFront()
        {
            return RemoveFront();
        }
    }
}