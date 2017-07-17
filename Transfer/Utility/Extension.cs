using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
    }
}