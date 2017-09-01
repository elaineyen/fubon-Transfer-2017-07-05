using System;
using System.Runtime.Serialization;

namespace Transfer.ViewModels
{
    /// <summary>
    /// Data Requirements (有dateTime型別故需此model做轉換)
    /// </summary>
    [DataContract]
    [Serializable]
    public class A41ViewModel
    {
        [DataMember]
        public string Reference_Nbr { get; set; }

        [DataMember]
        public string Bond_Number { get; set; }

        [DataMember]
        public string Lots { get; set; }

        [DataMember]
        public string Segment_Name { get; set; }

        [DataMember]
        public string Curr_Sp_Issuer { get; set; }

        [DataMember]
        public string Curr_Moodys_Issuer { get; set; }

        [DataMember]
        public string Curr_Fitch_Issuer { get; set; }

        [DataMember]
        public string Curr_Tw_Issuer { get; set; }

        [DataMember]
        public string Curr_Sp_Issue { get; set; }

        [DataMember]
        public string Curr_Moodys_Issue { get; set; }

        [DataMember]
        public string Curr_Fitch_Issue { get; set; }

        [DataMember]
        public string Curr_Tw_Issue { get; set; }

        [DataMember]
        public string Ori_Amount { get; set; }

        [DataMember]
        public string Current_Int_Rate { get; set; }

        [DataMember]
        public string Origination_Date { get; set; }

        [DataMember]
        public string Maturity_Date { get; set; }

        [DataMember]
        public string Principal_Payment_Method_Code { get; set; }

        [DataMember]
        public string Payment_Frequency { get; set; }

        [DataMember]
        public string Baloon_Freq { get; set; }

        [DataMember]
        public string Issuer_Area { get; set; }

        [DataMember]
        public string Industry_Sector { get; set; }

        [DataMember]
        public string Product { get; set; }

        [DataMember]
        public string Ias39_Category { get; set; }

        [DataMember]
        public string Principal { get; set; }
        [DataMember]
        public string Amort_Amt_Tw { get; set; }
        [DataMember]
        public string Interest_Receivable { get; set; }

        [DataMember]
        public string Interest_Receivable_tw { get; set; }

        [DataMember]
        public string Interest_Rate_Type { get; set; }

        [DataMember]
        public string Impair_Yn { get; set; }
        [DataMember]
        public string Eir { get; set; }

        [DataMember]
        public string Currency_Code { get; set; }

        [DataMember]
        public string Report_Date { get; set; }


        [DataMember]
        public string Issuer { get; set; }
        [DataMember]
        public string Country_Risk { get; set; }
        [DataMember]
        public string Ex_rate { get; set; }
        [DataMember]
        public string Lien_position { get; set; }
        [DataMember]
        public string Portfolio { get; set; }
        [DataMember]
        public string Asset_Seg { get; set; }
        [DataMember]
        public string Ori_Ex_rate { get; set; }
        [DataMember]
        public string Bond_Type { get; set; }
        [DataMember]
        public string Assessment_Sub_Kind { get; set; }
        [DataMember]
        public string Processing_Date { get; set; }
        [DataMember]
        public string Version { get; set; }
        [DataMember]
        public string Bond_Aera { get; set; }

        [DataMember]
        public string Asset_Type { get; set; }

        [DataMember]
        public string IH_OS { get; set; }

        [DataMember]
        public string Amount_TW_Ori_Ex_rate { get; set; }

        [DataMember]
        public string Amort_Amt_Ori_Tw { get; set; }

        [DataMember]
        public string Market_Value_Ori { get; set; }

        [DataMember]
        public string Market_Value_TW { get; set; }

        [DataMember]
        public string Value_date { get; set; }

        [DataMember]
        public string Portfolio_Name { get; set; }
    }
}