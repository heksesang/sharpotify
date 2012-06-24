using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Sharpotify.Util
{
    internal static class FloatUtils
    {
        public static bool TryParse(string s, out float result)
        {
            result = 0f;
            if (string.IsNullOrEmpty(s))
                return false;
            try
            {
                result = float.Parse(s);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static float CreateFromIntBits(int value)
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(value), 0);
        }
    }
}
