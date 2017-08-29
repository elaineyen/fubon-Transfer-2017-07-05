using System.ComponentModel;

namespace Transfer.Enum
{
    public partial class Ref
    {
        public enum Product_Code
        {
            /// <summary>
            /// 房貸
            /// </summary>
            [Description("Loan_1")]
            M,

            /// <summary>
            /// 債券A
            /// </summary>
            [Description("Bond_A")]
            B_A,

            /// <summary>
            /// 債券B
            /// </summary>
            [Description("Bond_B")]
            B_B,

            /// <summary>
            /// 債券P
            /// </summary>
            [Description("Bond_P")]
            B_P
        }
    }
}