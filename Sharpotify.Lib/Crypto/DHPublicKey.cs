using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpotify.Crypto
{
    public class DHPublicKey
    {
        private byte[] _key;

        /*public byte[] P
        {
            get;
            private set;
        }

        public byte[] G
        {
            get;
            private set;
        }*/

        public byte[] KeyExchangeData
        {
            get
            {
                return _key;
            }
        }

        public byte[] ToByteArray()
        {
            return KeyExchangeData;
        }

        /*
        public DHPublicKey(byte[] p, byte[]g, byte[] keyExcahngeData)
        {
            P = p;
            G = g;
            _key = keyExcahngeData;
        }*/

        public DHPublicKey(byte[] keyExcahngeData)
        {
            _key = keyExcahngeData;
        }

    }
}
