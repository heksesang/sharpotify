using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.Mentalis.Security.Cryptography;

namespace Sharpotify.Crypto
{
    public class DHPrivateKey
    {
        private DHParameters _key;

        public byte[] P { get { return _key.P; } }
        public byte[] G { get { return _key.G; } }
        public byte[] X { get { return _key.X; } }

        public DHPrivateKey(DHParameters dhPrivateParams)
        {
            _key = dhPrivateParams;
        }
    }
}
