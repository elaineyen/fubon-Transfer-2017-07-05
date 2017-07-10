using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Transfer.ViewModels
{
    /// <summary>
    /// Exhibit 前段顯示Model
    /// </summary>
    public class Exhibit10Model
    {
        public string Trailing { get; set; }
        public string Actual_Allcorp { get; set; }
        public string Baseline_forecast_Allcorp { get; set; }
        public string Pessimistic_Forecast_Allcorp { get; set; }
        public string Actual_SG { get; set; }
        public string Baseline_forecast_SG { get; set; }
        public string Pessimistic_Forecast_SG { get; set; }
    }
}