﻿using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Transfer.ViewModels
{
    /// <summary>
    /// Data Requirements (有dateTime型別故需此model做轉換)
    /// </summary>
    public class A41ViewModel
    {
        [Description("帳戶編號")]
        public string Reference_Nbr { get; set; }

        [Description("債券編號")]
        public string Bond_Number { get; set; }

        [Description("Lots")]
        public string Lots { get; set; }

        [Description("債券(資產)名稱")]
        public string Segment_Name { get; set; }

        [Description("Curr_Sp_Issuer")]
        public string Curr_Sp_Issuer { get; set; }

        [Description("Curr_Moodys_Issuer")]
        public string Curr_Moodys_Issuer { get; set; }

        [Description("Curr_Fitch_Issuer")]
        public string Curr_Fitch_Issuer { get; set; }

        [Description("Curr_Tw_Issuer")]
        public string Curr_Tw_Issuer { get; set; }

        [Description("Curr_Sp_Issue")]
        public string Curr_Sp_Issue { get; set; }

        [Description("Curr_Moodys_Issue")]
        public string Curr_Moodys_Issue { get; set; }

        [Description("Curr_Fitch_Issue")]
        public string Curr_Fitch_Issue { get; set; }

        [Description("Curr_Tw_Issue")]
        public string Curr_Tw_Issue { get; set; }

        [Description("原始金額")]
        public string Ori_Amount { get; set; }

        [Description("原始利率(票面利率)")]
        public string Current_Int_Rate { get; set; }

        [Description("債券購入(認列)日期")]
        public string Origination_Date { get; set; }

        [Description("債券到期日")]
        public string Maturity_Date { get; set; }

        [Description("現金流類型")]
        public string Principal_Payment_Method_Code { get; set; }

        [Description("票面利率週期")]
        public string Payment_Frequency { get; set; }

        [Description("可贖回次數")]
        public string Baloon_Freq { get; set; }

        [Description("Issuer所屬區域")]
        public string Issuer_Area { get; set; }

        [Description("對手產業別")]
        public string Industry_Sector { get; set; }

        [Description("SMF")]
        public string Product { get; set; }

        [Description("原公報分類")]
        public string Ias39_Category { get; set; }

        [Description("攤銷後之成本數(原幣)")]
        public string Principal { get; set; }

        [Description("攤銷後之成本數(報表日匯率台幣)")]
        public string Amort_Amt_Tw { get; set; }

        [Description("應收利息(原幣)")]
        public string Interest_Receivable { get; set; }

        [Description("應收利息(台幣)")]
        public string Interest_Receivable_tw { get; set; }

        [Description("利率類型(Fix, Float)")]
        public string Interest_Rate_Type { get; set; }

        [Description("是否有客觀減損證據")]
        public string Impair_Yn { get; set; }

        [Description("原始有效利率(買入殖利率)")]
        public string Eir { get; set; }

        [Description("債券幣別")]
        public string Currency_Code { get; set; }

        [Description("評估基準日/報導日")]
        public string Report_Date { get; set; }

        [Description("Issuer")]
        public string Issuer { get; set; }

        [Description("Country_Risk")]
        public string Country_Risk { get; set; }

        [Description("日匯率")]
        public string Ex_rate { get; set; }

        [Description("債券擔保順位")]
        public string Lien_position { get; set; }

        [Description("Portfolio")]
        public string Portfolio { get; set; }

        [Description("資產區隔")]
        public string Asset_Seg { get; set; }

        [Description("成本匯率")]
        public string Ori_Ex_rate { get; set; }

        [Description("債券種類")]
        public string Bond_Type { get; set; }

        [Description("Stage評估次分類")]
        public string Assessment_Sub_Kind { get; set; }

        [Description("資料處理日期")]
        public string Processing_Date { get; set; }

        [Description("資料版本")]
        public string Version { get; set; }

        [Description("國內/國外")]
        public string Bond_Aera { get; set; }

        [Description("金融商品分類")]
        public string Asset_Type { get; set; }

        [Description("自操/委外")]
        public string IH_OS { get; set; }

        [Description("帳列面額(成本匯率台幣)")]
        public string Amount_TW_Ori_Ex_rate { get; set; }

        [Description("攤銷後成本(成本匯率台幣)")]
        public string Amort_Amt_Ori_Tw { get; set; }

        [Description("市價(原幣)")]
        public string Market_Value_Ori { get; set; }

        [Description("市價(報表日匯率台幣)")]
        public string Market_Value_TW { get; set; }

        [Description("市價資料日期")]
        public string Value_date { get; set; }

        [Description("Portfolio英文")]
        public string Portfolio_Name { get; set; }
    }
}