using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ThriftClientPool.Exception
{
    public class NoBackendServiceException : SocketException
    {
        public NoBackendServiceException()
        {
            
        }

        public NoBackendServiceException(string message)
        {
            
        }
    }
}
