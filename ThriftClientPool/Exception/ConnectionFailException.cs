using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift;

namespace ThriftClientPool.Exception
{
    public class ConnectionFailException : ThriftException
    {
        public ConnectionFailException()
        {
           
        }

        public ConnectionFailException(string message):base(message)
        {
            
        }
    }
}
