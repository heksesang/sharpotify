using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sharpotify.Util
{
    internal static class Hex
    {
        public static byte[] ToBytes(string hex)
        {
            if (!IsHex(hex))
                throw new ArgumentException("Input string is not a valid hexadecimal string.");
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length - 1; i += 2)
            {
                bytes[i / 2] = byte.Parse(new string(new Char[] { hex[i], hex[i + 1] }), 
                    System.Globalization.NumberStyles.HexNumber);
            }
            return bytes;
        }

        public static string ToHex(byte[] bytes)
        {
            string hex = "";
            foreach (byte b in bytes)
                hex += b.ToString("X2");
            return hex;
        }

        public static bool IsHex(string hex)
        {
            return (hex.Length % 2 == 0) && Regex.IsMatch(hex, @"[0-9A-Fa-f]+");
        }
    }
}
