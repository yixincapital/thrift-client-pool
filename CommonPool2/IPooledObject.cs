using System;
using System.IO;
using DequeNet;

namespace CommonPool2
{
    public interface IPooledObject<T> : IComparable<IPooledObject<T>>
    {
        /// <summary>
        /// Obtain the underlying object that is wrapped by this instance of  PooledObject
        /// </summary>
        /// <returns>The wrapped object</returns>
        T GetObject();

        /// <summary>
        ///  Obtain the time (using the same basis as currentTimeMillis()) that this object was created.
        /// </summary>
        /// <returns>The creation time for the wrapped object</returns>
        long GetCreateTime();

        /// <summary>
        /// Obtain the time in milliseconds that this object last spent in the the
        /// active state (it may still be active in which case subsequent calls will
        /// return an increased value).
        /// </summary>
        /// <returns>The time in milliseconds last spent in the active state</returns>
        long GetActiveTimeMillis();

        /// <summary>
        /// Obtain the time in milliseconds that this object last spend in the the
        /// idle state (it may still be idle in which case subsequent calls will
        /// return an increased value).
        /// </summary>
        /// <returns> The time in milliseconds last spent in the idle state</returns>
        long GetIdleTimeMillis();

        /// <summary>
        /// Obtain the time the wrapped object was last borrowed.
        /// </summary>
        /// <returns>The time the object was last borrowed</returns>
        long GetLastBorrowTime();

        /// <summary>
        ///  Obtain the time the wrapped object was last returned.
        /// </summary>
        /// <returns> The time the object was last returned</returns>
        long GetLastReturnTime();

        /// <summary>
        /// eturn an estimate of the last time this object was used.  If the class
        /// of the pooled object implements |TrackedUse|,what is returned is
        /// the maximum of |TrackedUse#getLastUsed()| and
        /// |getLastBorrowTime()| ; otherwise this method gives the same
        /// value as |getLastBorrowTime()|.
        /// </summary>
        /// <returns>the last time this object was used</returns>
        long GetLastUsedTime();

        /// <summary>
        /// ttempt to place the pooled object in the
        /// PooledObjectState state
        /// </summary>
        /// <returns>true if the object was placed in the otherwise false</returns>
        bool StartEvictionTest();

        /// <summary>
        /// alled to inform the object that the eviction test has ended.       
        /// </summary>
        /// <param name="idleQueue">The queue of idle objects to which the object should be returned</param>
        /// <returns>Currently not used</returns>
       bool EndEvictionTest(Deque<IPooledObject<T>> idleQueue);

        /// <summary>
       /// Allocates the object.
        /// </summary>
       /// <returns>@return true if the original state was PooledObjectState#IDLE</returns>
       bool Allocate();

        /// <summary>
       /// Deallocates the object and sets it PooledObjectState#IDLE
       /// if it is currently PooledObjectState#ALLOCATED
        /// </summary>
       /// <returns>return if the state was PooledObjectState#ALLOCATED</returns>
        bool Deallocate();

        /// <summary>
        /// Sets the state to PooledObjectState#INVALID
        /// </summary>
        void Invalidate();

        /// <summary>
        // Is abandoned object tracking being used? If this is true the
        // implementation will need to record the stack trace of the last caller to
        // borrow this object.
        /// </summary>
        /// <param name="logAbandoned"></param>
        void SetLogAbandoned(bool logAbandoned);

        /// <summary>
        /// Record the current stack trace as the last time the object was used.
        /// </summary>
        void Use();

        /// <summary>
         // Prints the stack trace of the code that borrowed this pooled object and
        //the stack trace of the last code to use this object (if available) to
       //the supplied writer.
        /// </summary>
        /// <param name="writer"></param>
        void PrintStackTrace(StringWriter writer);

        /// <summary>
        /// Returns the state of this object.
        /// </summary>
        /// <returns>state</returns>
        PooledObjectState GetState();

        /// <summary>
        /// Marks the pooled object as abandoned.
        /// </summary>
        void MarkAbandoned();

        /// <summary>
        /// Marks the object as returning to the pool.
        /// </summary>
        void MarkReturning(); 
    }
}