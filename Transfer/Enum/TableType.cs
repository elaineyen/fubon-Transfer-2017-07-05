using System.ComponentModel;

namespace Transfer.Enum
{
    public partial class Ref
    {
        /// <summary>
        /// 所有Table
        /// </summary>
        public enum Table_Type
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
            /// Grade_Moody_Info
            /// </summary>
            [Description("Grade_Moody_Info")]
            A51,

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
            /// Moody_Recovery_Info
            /// </summary>
            [Description("Moody_Recovery_Info")]
            A61,

            /// <summary>
            /// Moody_LGD_Info
            /// </summary>
            [Description("Moody_LGD_Info")]
            A62,

            /// <summary>
            /// Moody_Tm_YYYY
            /// </summary>
            [Description("Moody_Tm_YYYY")]
            A71,

            /// <summary>
            /// Tm_Adjust_YYYY
            /// </summary>
            [Description("Tm_Adjust_YYYY")]
            A72,

            /// <summary>
            /// GM_YYYY
            /// </summary>
            [Description("GM_YYYY")]
            A73,

            /// <summary>
            /// Moody_Monthly_PD_Info
            /// </summary>
            [Description("Moody_Monthly_PD_Info")]
            A81,

            /// <summary>
            /// Moody_Quartly_PD_Info
            /// </summary>
            [Description("Moody_Quartly_PD_Info")]
            A82,

            /// <summary>
            /// Moody_Predit_PD_Info
            /// </summary>
            [Description("Moody_Predit_PD_Info")]
            A83,

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

            /// <summary>
            /// Rating_History
            /// </summary>
            [Description("Rating_History")]
            C02,
        }
    }
}