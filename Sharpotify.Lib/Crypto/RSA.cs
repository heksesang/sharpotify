using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Sharpotify.Crypto
{
    internal class RSA
    {
        #region fields

        #endregion

        #region methods

        //public static RSAKeyPair GenerateKeyPair(int keysize)
        //{
        //    var item = new RSACryptoServiceProvider(keysize);
        //    return new RSAKeyPair(
        //        new RSAPrivateKey(item.ExportParameters(true).D),
        //        new RSAPublicKey(item.ExportParameters(false).Modulus));
        //}
        public static RSAKeyPair GenerateKeyPair(int keysize)
        {
            RSACryptoServiceProvider provider = new RSACryptoServiceProvider(keysize);
            return new RSAKeyPair(new RSAPrivateKey(provider.ExportParameters(true).D), new RSAPublicKey(provider.ExportParameters(false).Modulus));
        }

 

        #endregion
    }
}
