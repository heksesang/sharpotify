using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpotify.Exceptions
{
    public class ProtocolException : Exception
    {
        public ProtocolException(string message)
            : base(message)
        {
        }

        public ProtocolException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
