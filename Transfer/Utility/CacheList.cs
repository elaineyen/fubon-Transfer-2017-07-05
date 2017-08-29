namespace Transfer.Utility
{
    /// <summary>
    /// cache命名 目的為不重複cache名稱 避免資料被覆蓋
    /// </summary>
    public static class CacheList
    {
        #region 資料庫資料

        public static string A41DbfileData { get; private set; }
        public static string A41ExcelfileData { get; private set; }
        public static string A58DbfileData { get; private set; }
        public static string A58DbMissfileData { get; private set; }
        public static string A59ExcelfileData { get; private set; }

        #endregion 資料庫資料

        #region 上傳檔名

        public static string A41ExcelName { get; private set; }

        public static string A42ExcelName { get; private set; }

        public static string A59ExcelName { get; private set; }

        public static string A62ExcelName { get; private set; }

        public static string A71ExcelName { get; private set; }

        public static string A81ExcelName { get; private set; }

        #endregion 上傳檔名

        static CacheList()
        {
            #region 資料庫資料

            A41ExcelfileData = "A41ExcelfileData";
            A41DbfileData = "A41DbfileData";
            A58DbfileData = "A58DbfileData";
            A58DbMissfileData = "A58DbMissfileData";
            A59ExcelfileData = "A59ExcelfileData";

            #endregion 資料庫資料

            #region 上傳檔名

            A41ExcelName = "A41ExcelName";
            A42ExcelName = "A42ExcelName";
            A59ExcelName = "A59ExcelName";
            A62ExcelName = "A62ExcelName";
            A71ExcelName = "A71ExcelName";
            A81ExcelName = "A81ExcelName";

            #endregion 上傳檔名
        }
    }
}