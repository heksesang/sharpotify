using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Sharpotify.Crypto
{
    public class RSAPublicKey
    {
        private byte[] _key;

        public byte[] ToByteArray()
        {
            return _key;
        }

        public RSAPublicKey(byte[] key)
        {
            _key = key;
        }
    }
}
