using System;
using System.Collections.Generic;
using System.IO;

namespace CommonPool2.impl
{   
    public class DefaultPooledObject<T>:IPooledObject<T>
    {
        private  readonly T currentObj; 
        private PooledObjectState state = PooledObjectState.Idle; // @GuardedBy("this") to ensure transitions are valid
        private static readonly long createTime = DateTime.Now.Millisecond;
        private  long lastBorrowTime = createTime;
        private  long lastUseTime = createTime;
        private  long lastReturnTime = createTime;
        private  bool logAbandoned = false;
        private volatile Exception borrowedBy = null;
        private volatile Exception usedBy = null;
        private  long borrowedCount = 0;
        private readonly object syncLock = new object();

        public DefaultPooledObject(T obj)
        {
            this.currentObj = obj;
        }

        public int CompareTo(IPooledObject<T> other)
        {
            long lastActiveDiff = this.GetLastReturnTime() - other.GetLastReturnTime();
            if (lastActiveDiff == 0)
            {
                // Make sure the natural ordering is broadly consistent with equals
                // although this will break down if distinct objects have the same
                // identity hash code.
                // see java.lang.Comparable Javadocs
                return this.GetHashCode() - other.GetHashCode();
            }
            // handle int overflow
            return (int)Math.Min(Math.Max(lastActiveDiff, 0x80000000), 0x7fffffff); // 0x80000000 is the min int 
        }

        public T GetObject()
        {
            return currentObj;
        }

        public long GetCreateTime()
        {
            return createTime;
        }

        public long GetActiveTimeMillis()
        {
            long rTime = lastReturnTime;
            long bTime = lastBorrowTime;

            if (rTime > bTime)
            {
                return rTime - bTime;
            }
            else
            {
                return DateTime.Now.Millisecond - bTime;       
            }               
        }

        public long GetIdleTimeMillis()
        {
            var elapsed = DateTime.Now.Millisecond - lastBorrowTime;
            return elapsed >= 0 ? elapsed : 0;
        }

        public long GetLastBorrowTime()
        {
            return lastBorrowTime;
        }

        public long GetLastReturnTime()
        {
            return lastReturnTime;
        }

        public long GetLastUsedTime()
        {
            if (currentObj.GetType() == typeof (ITrackedUse))
            {
                return Math.Max(((ITrackedUse) currentObj).GetLastUsed(), lastUseTime);
            }
            else
            {
                return lastUseTime;
            }
        }

         
        public bool StartEvictionTest()
        {
            if (state == PooledObjectState.Idle)
            {
                state = PooledObjectState.Eviction;
                return true;
            }

            return false;
        }

        public bool EndEvictionTest(Deque<IPooledObject<T>> idleQueue)
        {
             if (state == PooledObjectState.Eviction) {
            state = PooledObjectState.Idle;
            return true;
        } else if (state == PooledObjectState.EvictionReturnToHead) {
            state = PooledObjectState.Idle;
            //if (!idleQueue.offerFirst(this)) {
            //    // TODO - Should never happen
            }
             return false;
        }

        public bool Allocate()
        {
            if (state == PooledObjectState.Idle)
            {
                state = PooledObjectState.Allocated;
                lastBorrowTime = DateTime.Now.Millisecond;
                lastUseTime = lastBorrowTime;
                borrowedCount++;
                if (logAbandoned)
                {
                    borrowedBy = new AbandonedObjectCreatedException();
                }
                return true;
            }
            else if (state == PooledObjectState.Eviction)
            {
                // TODO Allocate anyway and ignore eviction test
                state = PooledObjectState.EvictionReturnToHead;
                return false;
            }
            // TODO if validating and testOnBorrow == true then pre-allocate for
            // performance
            return false;
        }

        public bool Deallocate()
        {
            if (state == PooledObjectState.Allocated ||
                state == PooledObjectState.Returning)
            {
                state = PooledObjectState.Idle;
                lastReturnTime = DateTime.Now.Millisecond;
                borrowedBy = null;
                return true;
            }

            return false;
        }

        public void Invalidate()
        {
            lock (syncLock)
            {
                state = PooledObjectState.Invalid;
            }
        }

        public void SetLogAbandoned(bool logAbandoned)
        {
            this.logAbandoned = logAbandoned;
        }

        public void Use()
        {
            lastUseTime = DateTime.Now.Millisecond;
            usedBy = new Exception("The last code to use this object was:");
        }

        public void PrintStackTrace(StringWriter writer)
        {
            //TODO use a custom writer do some print work :)
        }

        public PooledObjectState GetState()
        {
            lock (syncLock)
            {
                return state;
            }
        }

        public void MarkAbandoned()
        {
            lock (syncLock)
            {
                state = PooledObjectState.Abandoned;
            }
        }

        public void MarkReturning()
        {
            lock (syncLock)
            {
                state = PooledObjectState.Returning;
            }
        }
    }

     class AbandonedObjectCreatedException : Exception
    {
         private static readonly long serialVersionUID = 7398692158058772916L;
         private readonly long _createdTime;

         public AbandonedObjectCreatedException()
         {             
             _createdTime = DateTime.Now.Millisecond;
         }

         public string GetMessage()
         {
              string msg;
             msg = new DateTime(_createdTime).ToString("yyyy-MM-dd HH:mm:ss Z");     
            return msg;
         }
         
    }
}