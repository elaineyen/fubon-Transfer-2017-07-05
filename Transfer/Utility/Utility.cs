namespace Transfer.Utility
{
    public static class SetFile
    {
        static SetFile()
        {
            ProgramName = "Transfer";
            FileUploads = "FileUploads";
            FileDownloads = "FileDownloads";
            A41TransferTxtLog = @"DataRequirementsTransfer.txt";
            A42TransferTxtLog = @"A42Transfer.txt";
            A62TransferTxtLog = @"Exhibit 7Transfer.txt";
            A71TransferTxtLog = @"Exhibit29Transfer.txt";
            A81TransferTxtLog = @"Exhibit10Transfer.txt";
        }

        public static string A41TransferTxtLog { get; private set; }
        public static string A42TransferTxtLog { get; private set; }
        public static string A62TransferTxtLog { get; private set; }
        public static string A71TransferTxtLog { get; private set; }
        public static string A81TransferTxtLog { get; private set; }
        public static string FileDownloads { get; private set; }
        public static string FileUploads { get; private set; }
        public static string ProgramName { get; private set; }
    }
}