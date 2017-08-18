using System.ComponentModel;
using System.Xml.Serialization;

namespace Transfer.Enum
{
    public partial class Ref
    {
        /// <summary>
        /// 可以轉檔的Table
        /// </summary>
        public enum Transfer_Table_Type
        {
            /// <summary>
            /// Bond_Account_Info
            /// </summary>
            [Description("Bond_Account_Info")]
            A41,

            /// <summary>
            /// Treasury_Securities_Info
            /// </summary>
            [Description("Treasury_Securities_Info")]
            A42,

            /// <summary>
            /// Rating_Info
            /// </summary>
            [Description("Rating_Info")]
            A53,

            /// <summary>
            /// Bond_Rating_Info
            /// </summary>
            [Description("Bond_Rating_Info")]
            A57,

            /// <summary>
            /// Bond_Rating_Summary
            /// </summary>
            [Description("Bond_Rating_Summary")]
            A58,

            /// <summary>
            /// IFRS9_Main
            /// </summary>
            [Description("IFRS9_Main")]
            B01,

            /// <summary>
            /// EL_Data_In
            /// </summary>
            [Description("EL_Data_In")]
            C01,
        }
    }
}