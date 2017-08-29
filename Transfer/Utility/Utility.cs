namespace Transfer.Utility
{
    public static class SetFile
    {
        static SetFile()
        {
            ProgramName = "Transfer"; //專案名稱
            FileUploads = "FileUploads"; //上傳檔案放置位置
            FileDownloads = "FileDownloads"; //下載檔案放置位置
            A41TransferTxtLog = @"DataRequirementsTransfer.txt"; //A41上傳Txtlog檔名
            A42TransferTxtLog = @"A42Transfer.txt"; //A42上傳Txtlog檔名
            A59TransferTxtLog = @"A59Transfer.txt"; //A59上傳Txtlog檔名
            A62TransferTxtLog = @"Exhibit 7Transfer.txt"; //A62上傳Txtlog檔名
            A71TransferTxtLog = @"Exhibit29Transfer.txt"; //A71上傳Txtlog檔名
            A81TransferTxtLog = @"Exhibit10Transfer.txt"; //A81上傳Txtlog檔名
        }

        public static string A41TransferTxtLog { get; private set; }
        public static string A42TransferTxtLog { get; private set; }
        public static string A59TransferTxtLog { get; private set; }
        public static string A62TransferTxtLog { get; private set; }
        public static string A71TransferTxtLog { get; private set; }
        public static string A81TransferTxtLog { get; private set; }
        public static string FileDownloads { get; private set; }
        public static string FileUploads { get; private set; }
        public static string ProgramName { get; private set; }
    }
}