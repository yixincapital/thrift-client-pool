using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonPool2
{
    /// <summary>
    /// An interface defining life-cycle methods for instances to be served by an ObjectPool
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPooledObjectFactory<T>
    {
        /// <summary>
        /// Create an instance that can be served by the pool and wrap it in a PooledObject to be managed by the pool.
        /// </summary>
        /// <returns></returns>
        IPooledObject<T> MakeObject();

        /// <summary>
        /// Destroys an instance no longer needed by the pool. 
        /// </summary>
        /// <param name="p"></param>
        void DestroyObject(IPooledObject<T> p);

        /// <summary>
        /// Ensures that the instance is safe to be returned by the pool.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        bool ValidateObject(IPooledObject<T> p);

        /// <summary>
        /// Reinitialize an instance to be returned by the pool.
        /// </summary>
        /// <param name="p"></param>
        void ActivateObject(IPooledObject<T> p);

        /// <summary>
        /// Uninitialize an instance to be returned to the idle object pool.
        /// </summary>
        /// <param name="p"></param>
        void PassivateObject(IPooledObject<T> p);

    }
}
