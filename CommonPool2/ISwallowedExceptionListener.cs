using System;

namespace CommonPool2
{
    public interface ISwallowedExceptionListener
    {
        /**
    * This method is called every time the implementation unavoidably swallows
    * an exception.
    *
    * @param e The exception that was swallowed
    */
        void OnSwallowException(Exception e);  
    }
}