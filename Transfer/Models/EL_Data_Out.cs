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
    
    public partial class EL_Data_Out
    {
        public string PID { get; set; }
        public string Data_ID { get; set; }
        public string PRJID { get; set; }
        public string FLOWID { get; set; }
        public string Report_Date { get; set; }
        public string Processing_Date { get; set; }
        public string Product_Code { get; set; }
        public string Reference_Nbr { get; set; }
        public double PD { get; set; }
        public double Lifetime_EL { get; set; }
        public double Y1_EL { get; set; }
        public double EL { get; set; }
        public string Impairment_Stage { get; set; }
        public Nullable<System.DateTime> Exec_Date { get; set; }
        public int Version { get; set; }
    
        public virtual Group_Product_Code_Mapping Group_Product_Code_Mapping { get; set; }
    }
}
