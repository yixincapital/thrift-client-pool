using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonPool2
{
    public class IllegalStateException : Exception
    {
        public IllegalStateException(string message) : base(message)
        {
            
        }
    }
}
