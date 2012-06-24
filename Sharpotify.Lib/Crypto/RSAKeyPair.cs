using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpotify.Crypto
{
    public class RSAKeyPair
    {
        #region fields

        private RSAPrivateKey _privateKey;
        private RSAPublicKey _publicKey;

        #endregion

        #region properties

        public RSAPrivateKey PrivateKey
        {
            get
            {
                return this._privateKey;
            }
        }

        public RSAPublicKey PublicKey
        {
            get
            {
                return this._publicKey;
            }
        }

        #endregion

        #region construction

        public RSAKeyPair(RSAPrivateKey privateKey, RSAPublicKey publicKey)
        {
            this._privateKey = privateKey;
            this._publicKey = publicKey;
        }

        #endregion
    }
}
