using System.ComponentModel;
using System.Xml.Serialization;

namespace Transfer.Enum
{
    public partial class Ref
    {
        public enum PD_Type
        {
            /// <summary>
            /// Past_Year_AVG
            /// </summary>
            [Description("Past_Year_AVG")]
            Past_Year_AVG,

            /// <summary>
            /// Forcast
            /// </summary>
            [Description("Forcast")]
            Forcast,
        }
    }
}