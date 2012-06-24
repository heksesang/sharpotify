using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpotify.Crypto
{
    internal static class RandomBytes
    {
        private static Random _rnd = new Random();

        public static void GetRandomBytes(ref byte[] buffer)
        {
            _rnd.NextBytes(buffer);
        }

        public static byte[] GetRandomBytes(int length)
        {
            /* Create a buffer of the specified length. */
            byte[] buffer = new byte[length];
            GetRandomBytes(ref buffer);
            return buffer;
        }
    }
}
