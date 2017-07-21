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

        #region 時間相減 取年
        /// <summary>
        /// 時間相減 取年
        /// </summary>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <returns></returns>
        public static double? dateSubtractToYear(this DateTime? date1, DateTime? date2)
        {
            if (!date1.HasValue || !date2.HasValue)
            {
                return null;
            }
            TimeSpan t = date1.Value.Subtract(date2.Value);
            return t.GetYears();
        }
        #endregion

        #region 時間相減 取月
        /// <summary>
        /// 時間相減 取月
        /// </summary>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <returns></returns>
        public static int? dateSubtractToMonths(this DateTime? date1, DateTime? date2)
        {
            if (!date1.HasValue || !date2.HasValue)
            {
                return null;
            }
            TimeSpan t = date1.Value.Subtract(date2.Value);
            return t.GetMonths();
        }
        #endregion

        public static double GetYears(this TimeSpan timespan)
        {
            return (double)(timespan.Days / 365.2425);
        }

        public static int GetMonths(this TimeSpan timespan)
        {
            return (int)(timespan.Days / 30.436875);
        }

        public static string GetDescription<T>(this T enumerationValue,string title = null,string body = null)
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
                    if (!title.IsNullOrWhiteSpace() && !body.IsNullOrWhiteSpace())
                        return string.Format("{0} : {1} => {2}",
                            title,
                            ((DescriptionAttribute)attrs[0]).Description,
                            body
                            );
                    if (!title.IsNullOrWhiteSpace())
                        return string.Format("{0} : {1}",
                            title,
                            ((DescriptionAttribute)attrs[0]).Description
                            );
                    if (!body.IsNullOrWhiteSpace())
                        return string.Format("{0} => {1}",
                            ((DescriptionAttribute)attrs[0]).Description,
                            body
                            );
                     return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            return enumerationValue.ToString();
        }
    }
}