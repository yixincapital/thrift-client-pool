using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using CommonPool2.Extension;

namespace CommonPool2.impl
{
    public abstract class BaseGenericObjectPool<T>
    {
        public static readonly int MEAN_TIMING_STATS_CACHE_SIZE = 100;
        // Configuration attributes
        private volatile int maxTotal =
                GenericKeyedObjectPoolConfig.DEFAULT_MAX_TOTAL;
        private volatile bool blockWhenExhausted =
                BaseObjectPoolConfig.DEFAULT_BLOCK_WHEN_EXHAUSTED;
        private long maxWaitMillis =
                BaseObjectPoolConfig.DEFAULT_MAX_WAIT_MILLIS;
        private static bool lifo = BaseObjectPoolConfig.DEFAULT_LIFO;
        private readonly bool fairness;
        private volatile bool testOnCreate =
                BaseObjectPoolConfig.DEFAULT_TEST_ON_CREATE;
        private volatile bool testOnBorrow =
                BaseObjectPoolConfig.DEFAULT_TEST_ON_BORROW;
        private volatile bool testOnReturn =
                BaseObjectPoolConfig.DEFAULT_TEST_ON_RETURN;
        private volatile bool testWhileIdle =
                BaseObjectPoolConfig.DEFAULT_TEST_WHILE_IDLE;
        private long timeBetweenEvictionRunsMillis =
                BaseObjectPoolConfig.DEFAULT_TIME_BETWEEN_EVICTION_RUNS_MILLIS;
        private volatile int numTestsPerEvictionRun =
                BaseObjectPoolConfig.DEFAULT_NUM_TESTS_PER_EVICTION_RUN;
        private long minEvictableIdleTimeMillis =
                BaseObjectPoolConfig.DEFAULT_MIN_EVICTABLE_IDLE_TIME_MILLIS;
        private long softMinEvictableIdleTimeMillis =
                BaseObjectPoolConfig.DEFAULT_SOFT_MIN_EVICTABLE_IDLE_TIME_MILLIS;
        private volatile EvictionPolicy<T> evictionPolicy;


        // Internal (primarily state) attributes
        static readonly Object closeLock = new Object();

        bool closed = false;
        readonly Object evictionLock = new Object();
        EvictionIterator evictionIterator = null; // @GuardedBy("evictionLock")
        /*
         * Class loader for evictor thread to use since, in a JavaEE or similar
         * environment, the context class loader for the evictor thread may not have
         * visibility of the correct factory. See POOL-161. Uses a weak reference to
         * avoid potential memory leaks if the Pool is discarded rather than closed.
         */
        private readonly WeakReference<AppDomain> factoryClassLoader;

        // TODO I will find the jmx like instance in C#
        //// Monitoring (primarily JMX) attributes
        //private  ObjectName oname;
        //private  String creationStackTrace;
        protected int _borrowedCount = 0;
        protected int _returnedCount = 0;
        protected int createdCount = 0;
        int destroyedCount = 0;
        int destroyedByEvictorCount = 0;
        int destroyedByBorrowValidationCount = 0;
        protected readonly StatsStore activeTimes = new StatsStore(MEAN_TIMING_STATS_CACHE_SIZE);
        protected StatsStore idleTimes = new StatsStore(MEAN_TIMING_STATS_CACHE_SIZE);
        protected StatsStore waitTimes = new StatsStore(MEAN_TIMING_STATS_CACHE_SIZE);
        protected int maxBorrowWaitTimeMillis = 0;
        //private volatile SwallowedExceptionListener swallowedExceptionListener = null;

        protected BaseGenericObjectPool(BaseObjectPoolConfig config)
        {
        }

        protected BaseGenericObjectPool()
        {
            
        }

        /// <summary>
        /// Returns the maximum number of objects that can be allocated by the pool
        /// </summary>
        /// <returns></returns>
        public int GetMaxTotal()
        {
            return maxTotal;
        }

        /// <summary>
        /// Sets the cap on the number of objects that can be allocated by the pool
        /// </summary>
        /// <param name="maxTotal"></param>
        public void SetMaxTotal(int maxTotal)
        {
            this.maxTotal = maxTotal;
        }

        public bool getBlockWhenExhausted()
        {
            return blockWhenExhausted;
        }

        /// <summary>
        /// Sets whether to block when the <code>borrowObject()</code> method is
        /// nvoked when the pool is exhausted (the maximum number of "active" objects has been reached).
        /// </summary>
        /// <param name="blockWhenExhausted"></param>
        /// <returns></returns>
        public void SetBlockWhenExhausted(bool blockWhenExhausted)
        {
            this.blockWhenExhausted = blockWhenExhausted;
        }
        /// <summary>
        /// Returns the maximum amount of time (in milliseconds) the
        /// borrowObject() method should block before throwing an exception when the pool is exhausted and
        /// getBlockWhenExhausted is true.
        /// </summary>
        /// <returns></returns>
        public long GetMaxWaitMillis()
        {
            return maxWaitMillis;
        }

        public void SetMaxWaitMillis(long maxWaitMillis)
        {
            this.maxWaitMillis = maxWaitMillis;
        }

        /// <summary>
        /// Returns whether or not the pool serves threads waiting to borrow objects fairly.
        /// True means that waiting threads are served as if waiting in a FIFO queue.
        /// </summary>
        /// <returns></returns>
        public bool getFairness()
        {
            return fairness;
        }
        public void setLifo(bool lifo)
        {
            //TODO language problem solve it later
        }

        public bool getTestOnCreate()
        {
            return testOnCreate;
        }

        public void SetTestOnCreate(bool testOnCreate)
        {
            this.testOnCreate = testOnCreate;
        }

        public bool GetTestOnBorrow()
        {
            return testOnBorrow;
        }

        public void SetTestOnBorrow(bool testOnBorrow)
        {
            this.testOnBorrow = testOnBorrow;
        }

        public bool GetTestOnReturn()
        {
            return testOnReturn;
        }

        public void SetTestOnReturn(bool testOnReturn)
        {
            this.testOnReturn = testOnReturn;
        }
        public bool GetTestWhileIdle()
        {
            return testWhileIdle;
        }
        public void SetTimeBetweenEvictionRunsMillis(
            long timeBetweenEvictionRunsMillis)
        {
            this.timeBetweenEvictionRunsMillis = timeBetweenEvictionRunsMillis;
            // TODO  startEvictor(timeBetweenEvictionRunsMillis);  // use timer instead of timetask in C#
        }

        public int GetNumTestsPerEvictionRun()
        {
            return numTestsPerEvictionRun;
        }

        public void SetTestWhileIdle(bool testWhileIdle)
        {
            this.testWhileIdle = testWhileIdle;
        }
        public long GetTimeBetweenEvictionRunsMillis()
        {
            return timeBetweenEvictionRunsMillis;
        }

        /**
  * Sets the maximum number of objects to examine during each run (if any)
  * of the idle object evictor thread. When positive, the number of tests
  * performed for a run will be the minimum of the configured value and the
  * number of idle instances in the pool. When negative, the number of tests
  * performed will be <code>ceil({@link #getNumIdle}/
  * abs({@link #getNumTestsPerEvictionRun}))</code> which means that when the
  * value is <code>-n</code> roughly one nth of the idle objects will be
  * tested per run.
  ***/
        public void SetNumTestsPerEvictionRun(int numTestsPerEvictionRun)
        {
            this.numTestsPerEvictionRun = numTestsPerEvictionRun;
        }

        /// <summary>
        /// Returns the minimum amount of time an object may sit idle in the pool
        ///  before it is eligible for eviction by the idle object evictor (if any -
        ///  When non-positive, no objects will be evicted from the pool due to idle time alone.
        /// </summary>
        /// <returns></returns>
        public long GetMinEvictableIdleTimeMillis()
        {
            return minEvictableIdleTimeMillis;
        }
        public void SetMinEvictableIdleTimeMillis(
            long minEvictableIdleTimeMillis)
        {
            this.minEvictableIdleTimeMillis = minEvictableIdleTimeMillis;
        }

        public long GetSoftMinEvictableIdleTimeMillis()
        {
            return softMinEvictableIdleTimeMillis;
        }

        public void SetSoftMinEvictableIdleTimeMillis(
           long softMinEvictableIdleTimeMillis)
        {
            this.softMinEvictableIdleTimeMillis = softMinEvictableIdleTimeMillis;
        }

        public String GetEvictionPolicyClassName()
        {
            return evictionPolicy.GetType().FullName;
        }
        public void SetEvictionPolicyClassName(
           String evictionPolicyClassName)
        {
            throw new NotImplementedException();
        }
        public abstract void Close();

        public bool IsClosed()
        {
            return closed;
        }

        public abstract void Evict();

        protected EvictionPolicy<T> GetEvictionPolicy()
        {
            return evictionPolicy;
        }

        protected void AssertOpen()
        {

        }

        void StartEvictor(long delay)
        {
            lock (evictionLock)
            {

            }
            if (delay > 0)
            {
                //TODO  time elasped code here 
            }
        }

        protected abstract void EnsureMinIdle();

        public String GetCreationStackTrace()
        {
            throw new NotImplementedException();
        }
        public long GetBorrowedCount()
        {
            return _borrowedCount;
        }
        public long GetCreatedCount()
        {
            return createdCount;
        }

        /**
         * The total number of objects destroyed by this pool over the lifetime of
         * the pool.
         * @return the destroyed object count
         */
        public long GetDestroyedCount()
        {
            return destroyedCount;
        }

        /**
         * The total number of objects destroyed by the evictor associated with this
         * pool over the lifetime of the pool.
         * @return the evictor destroyed object count
         */
        public long GetDestroyedByEvictorCount()
        {
            return destroyedByEvictorCount;
        }

        /**
         * The total number of objects destroyed by this pool as a result of failing
         * validation during <code>borrowObject()</code> over the lifetime of the
         * pool.
         * @return validation destroyed object count
         */
        public long GetDestroyedByBorrowValidationCount()
        {
            return destroyedByBorrowValidationCount;
        }

        /**
         * The mean time objects are active for based on the last {@link
         * #MEAN_TIMING_STATS_CACHE_SIZE} objects returned to the pool.
         * @return mean time an object has been checked out from the pool among
         * recently returned objects
         */
        public long GetMeanActiveTimeMillis()
        {
            return activeTimes.GetMean();
        }

        /**
         * The mean time objects are idle for based on the last {@link
         * #MEAN_TIMING_STATS_CACHE_SIZE} objects borrowed from the pool.
         * @return mean time an object has been idle in the pool among recently
         * borrowed objects
         */
        public long GetMeanIdleTimeMillis()
        {
            return idleTimes.GetMean();
        }

        /**
         * The mean time threads wait to borrow an object based on the last {@link
         * #MEAN_TIMING_STATS_CACHE_SIZE} objects borrowed from the pool.
         * @return mean time in milliseconds that a recently served thread has had
         * to wait to borrow an object from the pool
         */
        public long GetMeanBorrowWaitTimeMillis()
        {
            return waitTimes.GetMean();
        }

        /**
         * The maximum time a thread has waited to borrow objects from the pool.
         * @return maximum wait time in milliseconds since the pool was created
         */
        public long GetMaxBorrowWaitTimeMillis()
        {
            return maxBorrowWaitTimeMillis;
        }

        /**
         * The number of instances currently idle in this pool.
         * @return count of instances available for checkout from the pool
         */
        public abstract int GetNumIdle();
        public long GetReturnedCount()
        {
            return _returnedCount;
        }


        private bool GetLifo()
        {
            return lifo;
        }

        void UpdateStatsBorrow(IPooledObject<T> p, long waitTime)
        {
            _borrowedCount++;
            idleTimes.Add(p.GetIdleTimeMillis());
            waitTimes.Add(waitTime);

            // TODO lock-free optimistic-locking maximum
            //long currentMax;
            //do {
            //    currentMax = maxBorrowWaitTimeMillis;
            //    if (currentMax >= waitTime) {
            //        break;
            //    }
            //} while (!maxBorrowWaitTimeMillis.CompareAndSet(currentMax, waitTime));
        }
        void UpdateStatsReturn(long activeTime)
        {
            _returnedCount++;
            activeTimes.Add(activeTime);
        }


        public class StatsStore
        {
            private readonly int[] values;
            private readonly int size;
            private int index;

            public StatsStore(int size)
            {
                this.size = size;
                values = new int[size];
                for (int i = 0; i < size; i++)
                {
                    values[i] = -1;
                }
            }

            /// <summary>
            /// Adds a value to the cache.  If the cache is full, one of the
            /// existing values is replaced by the new value.
            /// </summary>
            /// <param name="value"></param>
            public void Add(long value)
            {
                lock (closeLock)
                {
                    values[index] = (int)value;
                    index++;
                    if (index == size)
                    {
                        index = 0;
                    }
                }
            }

            /// <summary>
            /// Returns the mean of the cached values.
            /// </summary>
            /// <returns></returns>
            public long GetMean()
            {
                double result = 0;
                int counter = 0;
                for (int i = 0; i < size; i++)
                {
                    long value = values[i];
                    if (value != -1)
                    {
                        counter++;
                        result = result * ((counter - 1) / (double)counter) +
                                value / (double)counter;
                    }
                }
                return (long)result;
            }
        }

        private class EvictionIterator : IIterator<IPooledObject<T>>
        {
            private readonly Deque<IPooledObject<T>> idleObjects;
            private readonly IEnumerator<IPooledObject<T>> idleObjectIterator;
            private bool fetchedNext = false;
            private bool nextAvailable = false;
            private IPooledObject<T> next;
            EvictionIterator(Deque<IPooledObject<T>> idleObjects)
            {
                this.idleObjects = idleObjects;

                if (lifo)
                {
                    idleObjectIterator = idleObjects.Reverse().GetEnumerator();
                }
                else
                {
                    idleObjectIterator = idleObjects.GetEnumerator();
                }
            }


            public void Dispose()
            {
                idleObjectIterator.Dispose();
            }

            public bool HasNext
            {
                get
                {
                    CheckNext();
                    return nextAvailable;
                }
            }
            public IPooledObject<T> Next()
            {
                CheckNext();
                if (!nextAvailable)
                {
                    throw new InvalidOperationException();
                }
                fetchedNext = false; // We've consumed this now
                return next;
            }

            void CheckNext()
            {
                if (!fetchedNext)
                {
                    nextAvailable = idleObjectIterator.MoveNext();
                    if (nextAvailable)
                    {
                        next = idleObjectIterator.Current;
                    }
                    fetchedNext = true;
                }
            }

            public void Remove()
            {
                throw new NotSupportedException();
            }
        }

        /**
     * Wrapper for objects under management by the pool.
     *
     * GenericObjectPool and GenericKeyedObjectPool maintain references to all
     * objects under management using maps keyed on the objects. This wrapper
     * class ensures that objects can work as hash keys.
     *
     * @param <T> type of objects in the pool
     */
    protected class IdentityWrapper<T> {
        /** Wrapped object */
        private  readonly T instance;
        
        /**
         * Create a wrapper for an instance.
         *
         * @param instance object to wrap
         */
        public IdentityWrapper(T instance) {
            this.instance = instance;
        }
       
        public int HashCode() {
            return instance.GetHashCode();
        }


        public override bool Equals(object other) 
        {
          return  instance.Equals(other);
        }

        protected bool Equals(IdentityWrapper<T> other)
        {
            return EqualityComparer<T>.Default.Equals(instance, other.instance);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<T>.Default.GetHashCode(instance);
        }

        /**
         * @return the wrapped object
         */
        public T GetObject() {
            return instance; 
        }
    }
    }


}