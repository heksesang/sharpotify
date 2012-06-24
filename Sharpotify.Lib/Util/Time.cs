using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpotify.Util
{
    internal static class Time
    {
        public static Int64 GetUnixTimestamp()
        {
            return (Int64)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime()).TotalSeconds;
        }
    }
}
