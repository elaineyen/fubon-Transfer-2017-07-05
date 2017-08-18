using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Transfer.Enum;
using static Transfer.Enum.Ref;

namespace Transfer.Utility
{
    public static class CacheList
    {
        #region A4
        public static string A41ExcelfileData { get; private set; }

        public static string A41DbfileData { get; private set; }
        #endregion

        #region Excel Name

        public static string A41ExcelName { get; private set; }

        public static string A62ExcelName { get; private set; }

        public static string A71ExcelName { get; private set; }

        public static string A81ExcelName { get; private set; }
        #endregion

        static CacheList()
        {
            A41ExcelfileData = "A41ExcelfileData";
            A41DbfileData = "A41DbfileData";
            A41ExcelName = "A41ExcelName";
            A62ExcelName = "A62ExcelName";
            A71ExcelName = "A71ExcelName";
            A81ExcelName = "A81ExcelName";
        }
    }
}