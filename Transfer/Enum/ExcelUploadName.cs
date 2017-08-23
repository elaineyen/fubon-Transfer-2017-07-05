using System.ComponentModel;

namespace Transfer.Enum
{
    public partial class Ref
    {
        /// <summary>
        /// Excel檔案上傳名稱統一
        /// </summary>
        public enum Excel_UploadName
        {
            /// <summary>
            /// Data Requirements
            /// </summary>
            [Description("Data Requirements")]
            A41,

            /// <summary>
            /// A42
            /// </summary>
            [Description("A42")]
            A42,

            /// <summary>
            /// Exhibit 7
            /// </summary>
            [Description("Exhibit 7")]
            A62,

            /// <summary>
            /// Exhibit 29
            /// </summary>
            [Description("Exhibit 29")]
            A71,

            /// <summary>
            /// Exhibit 10
            /// </summary>
            [Description("Exhibit 10")]
            A81,
        }
    }
}