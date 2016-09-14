namespace CommonPool2
{
    public interface IUsageTracking<T>
    {    
        /// <summary>
        ///  This method is called every time a pooled object to enable the pool to
        ///  better track borrowed objects.
        /// </summary>
        /// <param name="pooledObject">he object that is being used</param>
        void Use(T pooledObject);  
    }
}