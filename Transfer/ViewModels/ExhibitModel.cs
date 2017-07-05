using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Transfer.ViewModels
{
    public class ExhibitModel
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