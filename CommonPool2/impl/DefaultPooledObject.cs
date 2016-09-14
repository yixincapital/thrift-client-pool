using System;
using System.IO;
using DequeNet;

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

        public DefaultPooledObject(T obj)
        {
            this.currentObj = obj;
        }

        public int CompareTo(IPooledObject<T> other)
        {
            throw new System.NotImplementedException();
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
            throw new System.NotImplementedException();
        }

        public bool EndEvictionTest(Deque<IPooledObject<T>> idleQueue)
        {
            throw new System.NotImplementedException();
        }

        public bool Allocate()
        {
            throw new System.NotImplementedException();
        }

        public bool Deallocate()
        {
            throw new System.NotImplementedException();
        }

        public void Invalidate()
        {
            throw new System.NotImplementedException();
        }

        public void SetLogAbandoned(bool logAbandoned)
        {
            throw new System.NotImplementedException();
        }

        public void Use()
        {
            throw new System.NotImplementedException();
        }

        public void PrintStackTrace(StringWriter writer)
        {
            throw new System.NotImplementedException();
        }

        public PooledObjectState GetState()
        {
            throw new System.NotImplementedException();
        }

        public void MarkAbandoned()
        {
            throw new System.NotImplementedException();
        }

        public void MarkReturning()
        {
            throw new System.NotImplementedException();
        }
    }
}