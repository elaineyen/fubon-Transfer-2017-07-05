using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Transfer.ViewModels
{
    /// <summary>
    /// Data Requirements (有dateTime型別故需此model做轉換)
    /// </summary>
    public class A41ViewModel
    {
        public string Reference_Nbr { get; set; }
        public string Bond_Number { get; set; }
        public string Lots { get; set; }
        public string Segment_Name { get; set; }
        public string Curr_Sp_Issuer { get; set; }
        public string Curr_Moodys_Issuer { get; set; }
        public string Curr_Fitch_Issuer { get; set; }
        public string Curr_Tw_Issuer { get; set; }
        public string Curr_Sp_Issue { get; set; }
        public string Curr_Moodys_Issue { get; set; }
        public string Curr_Fitch_Issue { get; set; }
        public string Curr_Tw_Issue { get; set; }
        public string Ori_Amount { get; set; }
        public string Current_Int_Rate { get; set; }
        public string Origination_Date { get; set; }
        public string Maturity_Date { get; set; }
        public string Principal_Payment_Method_Code { get; set; }
        public string Payment_Frequency { get; set; }
        public string Balloon_Date { get; set; }
        public string Issuer_Area { get; set; }
        public string Industry_Sector { get; set; }
        public string Product { get; set; }
        public string Finance_Instruments { get; set; }
        public string Ias39_Category { get; set; }
        public string Principal { get; set; }
        public string Amort_Amt_Tw { get; set; }
        public string Interest_Receivable { get; set; }
        public string Interest_Receivable_tw { get; set; }
        public string Interest_Rate_Type { get; set; }
        public string Impair_Yn { get; set; }
        public string Eir { get; set; }
        public string Currency_Code { get; set; }
        public string Report_Date { get; set; }
        public string Issuer { get; set; }
        public string Country_Risk { get; set; }
        public string Ex_rate { get; set; }
        public string Lien_position { get; set; }
        public string Portfolio { get; set; }
        public string Dept { get; set; }
        public string Asset_Seg { get; set; }
        public string Ori_Ex_rate { get; set; }
        public string Bond_Type { get; set; }
        public string Assessment_Sub_Kind { get; set; }
        public string Processing_Date { get; set; }
        public string Version { get; set; }
    }
}