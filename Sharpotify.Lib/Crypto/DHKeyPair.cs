using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpotify.Crypto
{
    public class DHKeyPair
    {
        #region fields

        private DHPrivateKey _privateKey;
        private DHPublicKey _publicKey;

        #endregion

        #region properties

        public DHPrivateKey PrivateKey
        {
            get
            {
                return this._privateKey;
            }
        }

        public DHPublicKey PublicKey
        {
            get
            {
                return this._publicKey;
            }
        }

        #endregion

        #region construction

        public DHKeyPair(DHPrivateKey privateKey, DHPublicKey publicKey)
        {
            this._privateKey = privateKey;
            this._publicKey = publicKey;
        }

        #endregion
    }
}
