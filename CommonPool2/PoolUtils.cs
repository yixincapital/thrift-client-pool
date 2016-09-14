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
}