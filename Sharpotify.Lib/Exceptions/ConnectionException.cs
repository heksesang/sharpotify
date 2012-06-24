using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpotify.Exceptions
{
    public class ConnectionException : Exception
    {
        public ConnectionException(string message)
            : base(message)
        {
        }

        public ConnectionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
