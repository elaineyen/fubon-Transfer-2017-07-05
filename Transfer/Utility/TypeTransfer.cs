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
    }
}