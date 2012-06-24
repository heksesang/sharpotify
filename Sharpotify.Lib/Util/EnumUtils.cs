using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Sharpotify.Util
{
    internal static class EnumUtils
    {
        public static string GetName(Type enumType, object value)
        {
            if (enumType.BaseType != typeof(Enum))
            {
                throw new ArgumentException("enumType parameter is not an System.Enum");
            }
            foreach (FieldInfo info in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                object obj2 = 0;
                try
                {
                    obj2 = Convert.ChangeType(info.GetValue(null), value.GetType(), null);
                }
                catch
                {
                    throw new ArgumentException();
                }
                if (obj2.Equals(value))
                {
                    return info.Name;
                }
            }
            return null;
        }
    }
}
