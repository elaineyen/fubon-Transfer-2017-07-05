using System;
using System.IO;
using System.Web;

namespace Transfer.Utility
{
    public static class TypeTransfer
    {
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
    }
}