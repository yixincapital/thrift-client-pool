using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using CommonPool2.Extension;
using DequeNet;

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
    //private final ObjectName oname;
    //private final String creationStackTrace;
    //private final AtomicLong borrowedCount = new AtomicLong(0);
    //private final AtomicLong returnedCount = new AtomicLong(0);
    //final AtomicLong createdCount = new AtomicLong(0);
    //final AtomicLong destroyedCount = new AtomicLong(0);
    //final AtomicLong destroyedByEvictorCount = new AtomicLong(0);
    //final AtomicLong destroyedByBorrowValidationCount = new AtomicLong(0);
    //private readonly StatsStore activeTimes = new StatsStore(MEAN_TIMING_STATS_CACHE_SIZE);
    //private final StatsStore idleTimes = new StatsStore(MEAN_TIMING_STATS_CACHE_SIZE);
    //private final StatsStore waitTimes = new StatsStore(MEAN_TIMING_STATS_CACHE_SIZE);
    //private final AtomicLong maxBorrowWaitTimeMillis = new AtomicLong(0L);
    //private volatile SwallowedExceptionListener swallowedExceptionListener = null;

        private bool GetLifo() {
        return lifo; 
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
                    values[index] = (int) value;
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

        private  class EvictionIterator : IIterator<IPooledObject<T>>
        {
            private readonly Deque<IPooledObject<T>> idleObjects;
            private readonly IEnumerator<IPooledObject<T>> idleObjectIterator;
            private bool fetchedNext = false;
            private bool nextAvailable = false;
            private IPooledObject<T> next;
             EvictionIterator(Deque<IPooledObject<T>> idleObjects) {
            this.idleObjects = idleObjects;

            if (lifo) {
                idleObjectIterator = idleObjects.Reverse().GetEnumerator();
            } else
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
    }

    
}