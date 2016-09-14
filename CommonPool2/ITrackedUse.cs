namespace CommonPool2
{
    public interface ITrackedUse
    {
        /// <summary>
        /// Get the last time this object was used in ms.
        /// </summary>
        /// <returns>long time in ms</returns>
        long GetLastUsed(); 
    }
}