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
            /// 債券明細檔
            /// </summary>
            [Description("Bond_Account_Info")]
            A41,

            /// <summary>
            /// 國庫券月結資料檔
            /// </summary>
            [Description("Treasury_Securities_Info")]
            A42,

            /// <summary>
            /// 信評主標尺對應檔_Moody
            /// </summary>
            [Description("Grade_Moody_Info")]
            A51,

            /// <summary>
            /// 外部信評資料檔
            /// </summary>
            [Description("Rating_Info")]
            A53,

            /// <summary>
            /// 債券信評檔_歷史檔
            /// </summary>
            [Description("Bond_Rating_Info")]
            A57,

            /// <summary>
            /// 債券信評檔_整理檔
            /// </summary>
            [Description("Bond_Rating_Summary")]
            A58,

            /// <summary>
            /// 債券信評補登批次檔
            /// </summary>
            [Description("Bond_Rating_Update_Info")]
            A59,

            /// <summary>
            /// 回收率資料檔_Moody
            /// </summary>
            [Description("Moody_Recovery_Info")]
            A61,

            /// <summary>
            /// 違約損失資料檔_歷史資料
            /// </summary>
            [Description("Moody_LGD_Info")]
            A62,

            /// <summary>
            /// 轉移矩陣資料檔_Moody
            /// </summary>
            [Description("Moody_Tm_YYYY")]
            A71,

            /// <summary>
            /// 轉移矩陣資料檔_調整後
            /// </summary>
            [Description("Tm_Adjust_YYYY")]
            A72,

            /// <summary>
            /// 等級違約率矩陣
            /// </summary>
            [Description("GM_YYYY")]
            A73,

            /// <summary>
            /// 月違約機率資料檔_Moody
            /// </summary>
            [Description("Moody_Monthly_PD_Info")]
            A81,

            /// <summary>
            /// 季違約機率資料檔_Moody
            /// </summary>
            [Description("Moody_Quartly_PD_Info")]
            A82,

            /// <summary>
            /// 機率預測資料_Moody
            /// </summary>
            [Description("Moody_Predit_PD_Info")]
            A83,

            /// <summary>
            /// 帳戶主檔
            /// </summary>
            [Description("IFRS9_Main")]
            B01,

            /// <summary>
            /// 減損計算輸入資料
            /// </summary>
            [Description("EL_Data_In")]
            C01,

            /// <summary>
            /// 信評歷史資料
            /// </summary>
            [Description("Rating_History")]
            C02,
        }
    }
}