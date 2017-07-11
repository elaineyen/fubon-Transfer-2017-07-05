using System;
using System.IO;
using System.Web;

namespace Transfer.Utility
{
    public static class TypeTransfer
    {
        #region Double? To String
        /// <summary>
        /// Double? 轉string (null 回傳 string.Empty)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string doubleToString(double? value)
        {
            if (value.HasValue)
                return value.Value.ToString();
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
    }
}