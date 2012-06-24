using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Sharpotify.Crypto
{
    internal static class Hash
    {
        public static byte[] Sha1(byte[] buffer)
        {
            return new SHA1CryptoServiceProvider().ComputeHash(buffer);
        }

        public static byte[] Md5(byte[] buffer)
        {
            return new MD5CryptoServiceProvider().ComputeHash(buffer);
        }

        public static byte[] HmacSha1(byte[] buffer, byte[] key)
        {
            
            return new HMACSHA1(key).ComputeHash(buffer);
        }

        public static void HmacSha1(byte[] buffer, byte[] key, byte[] output, int offset) 
        {
            byte[] hash = HmacSha1(buffer, key);
            Array.Copy(hash, 0, output, offset, hash.Length);
        }
    }
}
