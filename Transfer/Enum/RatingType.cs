using System.ComponentModel;

namespace Transfer.Enum
{
    public partial class Ref
    {
        public enum Rating_Type
        {
            /// <summary>
            /// 原始投資信評 (資料庫顯示為1)
            /// </summary>
            [Description("原始投資信評")]
            A,

            /// <summary>
            /// 評估日最近信評 (資料庫顯示為2)
            /// </summary>
            [Description("評估日最近信評")]
            B,
        }
    }
}