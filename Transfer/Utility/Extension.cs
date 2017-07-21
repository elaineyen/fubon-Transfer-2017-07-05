using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Transfer.Utility
{
    public static class Extension
    {
        public static IEnumerable<T> Filter<T>
            (this IEnumerable<T> data , Func<T,bool> fun)
        {
            foreach (T item in data)
            {
                if (fun(item))
                    yield return item; 
            }
        }

        public static bool IsNullOrWhiteSpace
            (this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        public static bool IsNullOrEmpty
             (this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static string GetDescription<T>(this T enumerationValue)
          where T : struct
        {
            var type = enumerationValue.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException($"{nameof(enumerationValue)} must be of Enum type", nameof(enumerationValue));
            }
            var memberInfo = type.GetMember(enumerationValue.ToString());
            if (memberInfo.Length > 0)
            {
                var attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            return enumerationValue.ToString();
        }
    }
}