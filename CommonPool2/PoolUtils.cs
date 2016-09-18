using System;
using System.Timers;
namespace CommonPool2
{
    public class PoolUtils
    {
        static class TimerHolder {
        static  Timer _minIdleTimer = new Timer();
         }
        public PoolUtils()
        {
        } 
    }

    public static class DateTimeExtensions
    {
        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static long CurrentTimeMillis(this DateTime d) 
        {
            return (long)((DateTime.UtcNow - Jan1st1970).TotalMilliseconds);
        }
    }
}