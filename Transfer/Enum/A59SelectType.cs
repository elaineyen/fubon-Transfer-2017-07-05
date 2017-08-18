using System.ComponentModel;
using System.Xml.Serialization;

namespace Transfer.Enum
{
    public partial class Ref 
    {
        public enum A59_SelectType
        {
            /// <summary>
            /// 報導日+選單條件(購買日評等、基準日最近評等) 債券編號可不輸入
            /// </summary>
            [Description("報導日+選單條件")]
            T1,

            /// <summary>
            /// 購買日+債券編號
            /// </summary>
            [Description("購買日+債券編號")]
            T2,

            /// <summary>
            /// 報導日+選單條件(購買日評等、基準日最近評等)+債券編號 購買日可不輸入
            /// </summary>
            [Description("報導日+選單條件+債券編號")]
            T3,

            /// <summary>
            /// 選單條件 (購買日評等、基準日最近評等)+債券編號+購買日 報導日可不輸入
            /// </summary>
            [Description("選單條件+債券編號+購買日")]
            T4,
        }
    }
}