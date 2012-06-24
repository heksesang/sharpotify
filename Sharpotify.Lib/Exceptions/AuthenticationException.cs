using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpotify.Exceptions
{
    public class AuthenticationException : Exception
    {
        public AuthenticationException(Exception innerException)
            : base(null, innerException)
        {
        }
        public AuthenticationException(string message)
            : base(message)
        {
        }
    }
}
