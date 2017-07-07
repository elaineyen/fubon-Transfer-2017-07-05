using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Transfer.ViewModels
{
    /// <summary>
    /// A81 view Data (由於A81有dateTime型別故需此model做轉換)
    /// </summary>
    [DataContract]
    public class A81ViewModel
    {
        [DataMember]
        public string Trailing_12m_Ending { get; set; }
        [DataMember]
        public string Actual_Allcorp { get; set; }
        [DataMember]
        public string Baseline_forecast_Allcorp { get; set; }
        [DataMember]
        public string Pessimistic_Forecast_Allcorp { get; set; }
        [DataMember]
        public string Actual_SG { get; set; }
        [DataMember]
        public string Baseline_forecast_SG { get; set; }
        [DataMember]
        public string Pessimistic_Forecast_SG { get; set; }
        [DataMember]
        public string Data_Year { get; set; }
    }
}