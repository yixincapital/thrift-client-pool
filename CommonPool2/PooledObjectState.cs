using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonPool2
{
    public enum PooledObjectState
    {
        /**
          * In the queue, not in use.
          */
        Idle,

        /**
         * In use.
         */
        Allocated,

        /**
         * In the queue, currently being tested for possible eviction.
         */
        Eviction,

        /**
         * Not in the queue, currently being tested for possible eviction. An
         * attempt to borrow the object was made while being tested which removed it
         * from the queue. It should be returned to the head of the queue once
         * eviction testing completes.
         * TODO: Consider allocating object and ignoring the result of the eviction
         *       test.
         */
        EvictionReturnToHead,

        /**
         * In the queue, currently being validated.
         */
        Validation,

        /**
         * Not in queue, currently being validated. The object was borrowed while
         * being validated and since testOnBorrow was configured, it was removed
         * from the queue and pre-allocated. It should be allocated once validation
         * completes.
         */
        ValidationPreallocated,

        /**
         * Not in queue, currently being validated. An attempt to borrow the object
         * was made while previously being tested for eviction which removed it from
         * the queue. It should be returned to the head of the queue once validation
         * completes.
         */
        ValidationReturnToHead,

        /**
         * Failed maintenance (e.g. eviction test or validation) and will be / has
         * been destroyed
         */
        Invalid,

        /**
         * Deemed abandoned, to be invalidated.
         */
        Abandoned, 

        /**
         * Returning to the pool.
         */
        Returning

    }
}
