using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpotify.Protocol
{
    internal class HostnamePortPair
    {
        private string _hostname = null;
        private int _port = 0;

        public string Hostname { get { return _hostname; } }
        public int Port { get { return _port; } }

        public HostnamePortPair(string hostname, int port)
        {
            _hostname = hostname;
            _port = port;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Hostname, Port);
        }
    }
}
