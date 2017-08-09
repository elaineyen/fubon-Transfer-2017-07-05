//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Transfer.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Bond_Account_Info
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
        public Nullable<double> Ori_Amount { get; set; }
        public Nullable<double> Current_Int_Rate { get; set; }
        public Nullable<System.DateTime> Origination_Date { get; set; }
        public Nullable<System.DateTime> Maturity_Date { get; set; }
        public string Principal_Payment_Method_Code { get; set; }
        public string Payment_Frequency { get; set; }
        public string Balloon_Date { get; set; }
        public string Issuer_Area { get; set; }
        public string Industry_Sector { get; set; }
        public string Product { get; set; }
        public string Finance_Instruments { get; set; }
        public string Ias39_Category { get; set; }
        public Nullable<double> Principal { get; set; }
        public Nullable<double> Amort_Amt_Tw { get; set; }
        public Nullable<double> Interest_Receivable { get; set; }
        public Nullable<double> Interest_Receivable_tw { get; set; }
        public string Interest_Rate_Type { get; set; }
        public string Impair_Yn { get; set; }
        public Nullable<double> Eir { get; set; }
        public string Currency_Code { get; set; }
        public Nullable<System.DateTime> Report_Date { get; set; }
        public string Issuer { get; set; }
        public string Country_Risk { get; set; }
        public Nullable<double> Ex_rate { get; set; }
        public string Lien_position { get; set; }
        public string Portfolio { get; set; }
        public string Dept { get; set; }
        public string Asset_Seg { get; set; }
        public Nullable<double> Ori_Ex_rate { get; set; }
        public string Bond_Type { get; set; }
        public string Assessment_Sub_Kind { get; set; }
        public Nullable<System.DateTime> Processing_Date { get; set; }
        public string Version { get; set; }
        public string Bond_Aera { get; set; }
        public string Asset_Type { get; set; }
        public string IH_OS { get; set; }
        public Nullable<double> Amount_TW_Ori_Ex_rate { get; set; }
        public Nullable<double> Amort_Amt_Ori_Tw { get; set; }
        public Nullable<double> Market_Value_Ori { get; set; }
        public Nullable<double> Market_Value_TW { get; set; }
        public Nullable<System.DateTime> Value_date { get; set; }
    }
}
