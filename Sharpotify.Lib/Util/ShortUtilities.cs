using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpotify.Util
{
    internal static class ShortUtilities
    {
        public static byte[] ToBytes(Int16 i)
        {
            byte[] b = new byte[2];
            b[0] = (byte)(i >> 8);
            b[1] = (byte)(i);
            return b;
        }

        public static UInt16 BytesToUnsignedShort(byte[] b, int offset)
        {
		    /* Check length of byte array. */
		    if(b.Length < offset + 2)
			    throw new ArgumentException("Not enough bytes in array.");
		
		    /* Convert and return value. */
		    return (UInt16)(((b[offset] << 8) & 0xFFFF) | ((b[offset + 1]) & 0x00FF));
        }

        public static short BytesToShort(byte[] b, int offset)
        {
            if (b.Length < offset + 2)
                throw new ArgumentException("Not enough bytes in array.");

            /* Convert and return value. */
            return (Int16)(((b[offset] << 8) & 0xFFFF) | (b[offset + 1]) & 0x00FF);
        }

        /* WRONG ENDIAN:
        public static byte[] ToBytes(Int16 i)
        {
            return BitConverter.GetBytes(i);
        }

        public static UInt16 BytesToUnsignedShort(byte[] b, int offset)
        {
            return BitConverter.ToUInt16(b, offset);
        }

        public static short BytesToShort(byte[] b, int offset)
        {
            return BitConverter.ToInt16(b, offset);
        }*/

        public static UInt16 BytesToUnsignedShort(byte[] b)
        {
            return BytesToUnsignedShort(b, 0);
        }

        public static Int16 BytesToShort(byte[] b)
        {
            return BytesToShort(b, 0);
        }
    }
}
