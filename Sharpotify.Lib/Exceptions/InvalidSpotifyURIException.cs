using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpotify.Exceptions
{
    public class InvalidSpotifyURIException : Exception
    {
        public InvalidSpotifyURIException(Exception innerException)
            : base(null, innerException)
        {
        }

        public InvalidSpotifyURIException()
            : base()
        {
        }
    }
}
