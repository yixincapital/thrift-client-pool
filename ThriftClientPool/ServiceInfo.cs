using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThriftClientPool
{
    public class ServiceInfo
    {
        private readonly string _host;
        private readonly int _port;

        public ServiceInfo(string host, int port)
        {
            this._host = host;
            this._port = port;
        }

        public string GetHost()
        {
            return _host;
        }

        public int GetPort()
        {
            return _port;
        }

        public int HashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + ((_host == null) ? 0 : _host.GetHashCode());
            result = prime * result + _port;
            return result;
        }

        public override bool Equals(object obj) 
        {
            if (this == obj) return true;
            if (obj == null) return false;
            if (this.GetType() != obj.GetType()) return false;
            ServiceInfo other = (ServiceInfo)obj;
            if (_host == null)
            {
                if (other._host != null) return false;
            }
            else if (!_host.Equals(other._host)) return false;
            if (_port != other._port) return false;
            return true;
        }

        protected bool Equals(ServiceInfo other)
        {
            return string.Equals(_host, other._host) && _port == other._port;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_host != null ? _host.GetHashCode() : 0)*397) ^ _port;
            }
        }

        public override String ToString()
        { 
            return "ServiceInfo [host=" + _host + ", port=" + _port + "]";
        }

    }
}
