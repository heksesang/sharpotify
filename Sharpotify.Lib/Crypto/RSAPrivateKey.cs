using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpotify.Crypto
{
    public class RSAPrivateKey
    {
        private byte[] _key;

        public byte[] ToByteArray()
        {
            return _key;
        }

        public RSAPrivateKey(byte[] key)
        {
            _key = key;
        }
    }
}
