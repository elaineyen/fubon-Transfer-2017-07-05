using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;

namespace Transfer.Utility
{
    public static class TypeTransfer
    {
        #region String To Int?
        /// <summary>
        /// string 轉 Int?
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int? stringToIntN(string value)
        {
            int i = 0;
            if (Int32.TryParse(value, out i))
                return i;
            return null;
        }
        #endregion

        #region String To Double 
        /// <summary>
        /// string 轉 Double
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double stringToDouble(string value)
        {
            double d = 0d;
            double.TryParse(value, out d);
            return d;
        }
        #endregion

        #region String To Double?
        /// <summary>
        /// string 轉 Double?
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double? stringToDoubleN(string value)
        {
            double d = 0d;
            if (double.TryParse(value, out d))
                return d;
            return null;
        }
        #endregion

        #region string (XX.XX%) To Double?
        /// <summary>
        /// string (XX.XX%) To Double?
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double? stringToDoubleNByP(string value)
        {
            double d = 0d;
            if (!value.IsNullOrWhiteSpace() &&
                value.EndsWith("%") &&
                double.TryParse(value.Split('%')[0], out d))
                return d / 100;
            return null;
        }
        #endregion

        #region String To DateTime?
        /// <summary>
        /// string 轉 DateTime?
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime? stringToDateTimeN(string value)
        {
            DateTime t = new DateTime();
            if (DateTime.TryParse(value, out t))
                return t;
            return null;
        }
        #endregion

        #region 民國字串轉西元年
        /// <summary>
        /// 民國字串轉西元年
        /// </summary>
        /// <param name="value">1051119 to 2016/11/19</param>
        /// <returns></returns>
        public static DateTime? stringToADDateTimeN(string value)
        {
            if (!value.IsNullOrWhiteSpace() && value.Length > 6)
            {
                string y = value.Substring(0, value.Length - 4);
                string m = value.Substring(value.Length - 4, 2);
                string d = value.Substring(value.Length - 2, 2);

                int ady = 0;
                Int32.TryParse(y, out ady);
                ady += 1911;
                y = ady.ToString();

                DateTime t = new DateTime();
                if (DateTime.TryParse($"{y}/{m}/{d}", out t))
                    return t;
                return null;
            }
            return null;
        }
        #endregion

        #region Int? To String
        /// <summary>
        /// Int? 轉string (null 回傳 string.Empty)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string intNToString(int? value)
        {
            if (value.HasValue)
                return value.Value.ToString();
            return string.Empty;
        }
        #endregion

        #region Int? To Int
        /// <summary>
        /// Int? 轉 Int (null 回傳 0)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int intNToInt(int? value)
        {
            if (value.HasValue)
                return value.Value;
            return 0;
        }
        #endregion

        #region Double? To String
        /// <summary>
        /// Double? 轉string (null 回傳 string.Empty)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string doubleNToString(double? value)
        {
            if (value.HasValue)
                return value.Value.ToString();
            return string.Empty;
        }
        #endregion

        #region Double? To Double
        /// <summary>
        /// Double? 轉 Double (null 回傳 0d)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double doubleNToDouble(double? value)
        {            
            if (value.HasValue)
                return value.Value;
            return 0d;
        }
        #endregion

        #region DateTime? To String
        /// <summary>
        /// DateTime? 轉string (null 回傳 string.Empty)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string dateTimeNToString(DateTime? value)
        {
            if (value.HasValue)
                return value.Value.ToString("yyyy/MM/dd");
            return string.Empty;
        }
        #endregion

        #region obj To String
        /// <summary>
        /// object 轉string (null 回傳 string.Empty)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string objToString(object value)
        {
            if (value != null)
                return value.ToString();
            return string.Empty;
        }
        #endregion

        #region obj To double
        /// <summary>
        /// object 轉 double
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double objToDouble(object value)
        {
            double d = 0d;
            if (value != null)
                double.TryParse(value.ToString(), out d);
            return d;
        }
        #endregion

        #region  obj To double?  (轉string包含%)
        /// <summary>
        /// object 轉 double? (轉string包含%)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double? objToDoubleNByP(object value)
        {
            double d = 0d;
            if (value != null)
                if (value.ToString().EndsWith("%"))
                    if (double.TryParse(value.ToString().Split('%')[0], out d))
                        return d/100;
            return null;
        }
        #endregion

        #region objDataToString(yyyy/MM/dd)
        public static string objDateToString(object value)
        {
            DateTime date = DateTime.MinValue;
            if (value != null && DateTime.TryParse(value.ToString(), out date))
                return date.ToString("yyyy/MM/dd");
            return string.Empty;
        }
        #endregion

        #region data To JsonString
        public static string dataToJson<T>(List<T> datas)
        {
            return new JavaScriptSerializer().Serialize(datas);
        }
        #endregion

        #region DoubleN multiplication
        public static double? DoubleNMultip(double? d1, double? d2)
        {
            if (d1.HasValue && d2.HasValue)
            {
                return d1.Value * d2.Value;
            }
            return null;
        }
        #endregion
    }
}