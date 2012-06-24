using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpotify.Util
{
    internal static class IntUtils
    {
        public static bool TryParse(string s, out int result)
        {
            result = 0;
            if (string.IsNullOrEmpty(s))
                return false;
            try
            {
                result = int.Parse(s);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static byte[] ToBytes(int i)
        {
            byte[] b = new byte[4];
            b[0] = (byte)(i >> 24);
            b[1] = (byte)(i >> 16);
            b[2] = (byte)(i >> 8);
            b[3] = (byte)(i);
            return b;
        }
        public static byte[] ToBytes(uint i)
        {
            byte[] b = new byte[4];
            b[0] = (byte)(i >> 24);
            b[1] = (byte)(i >> 16);
            b[2] = (byte)(i >> 8);
            b[3] = (byte)(i);
            return b;
        }

        public static UInt32 BytesToUnsignedInteger(byte[] b, int offset)
        {
            //FIXME: Ugly
            byte[] buffer = new byte[4];
            Array.Copy(b, offset, buffer, 0, buffer.Length);
            Array.Reverse(buffer);
            return BitConverter.ToUInt32(buffer, 0);
        }

        public static int BytesToInteger(byte[] b, int offset)
        {
            //FIXME: Ugly
            byte[] buffer = new byte[4];
            Array.Copy(b, offset, buffer, 0, buffer.Length);
            Array.Reverse(buffer);
            return BitConverter.ToInt32(buffer, 0);
        }

        /* WRONG ENDIAN
        public static byte[] ToBytes(int i)
        {
            return BitConverter.GetBytes(i);
        }

        public static UInt32 BytesToUnsignedInteger(byte[] b, int offset)
        {
            return BitConverter.ToUInt32(b, offset);
        }

        public static int BytesToInteger(byte[] b, int offset)
        {
            return BitConverter.ToInt32(b, offset);
        }*/

        public static UInt32 BytesToUnsignedInteger(byte[] b)
        {
            return BytesToUnsignedInteger(b, 0);
        }

        public static int BytesToInteger(byte[] b)
        {
            return BytesToInteger(b, 0);
        }
        
    }
}
