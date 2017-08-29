﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class IFRS9Entities : DbContext
    {
        public IFRS9Entities()
            : base("name=IFRS9Entities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<IFRS9_Log> IFRS9_Log { get; set; }
        public virtual DbSet<Moody_Monthly_PD_Info> Moody_Monthly_PD_Info { get; set; }
        public virtual DbSet<Moody_Predit_PD_Info> Moody_Predit_PD_Info { get; set; }
        public virtual DbSet<Moody_Quartly_PD_Info> Moody_Quartly_PD_Info { get; set; }
        public virtual DbSet<Moody_Tm_YYYY> Moody_Tm_YYYY { get; set; }
        public virtual DbSet<Grade_Moody_Info> Grade_Moody_Info { get; set; }
        public virtual DbSet<IFRS9_Menu_Main> IFRS9_Menu_Main { get; set; }
        public virtual DbSet<IFRS9_Menu_Set> IFRS9_Menu_Set { get; set; }
        public virtual DbSet<IFRS9_Menu_Sub> IFRS9_Menu_Sub { get; set; }
        public virtual DbSet<IFRS9_User> IFRS9_User { get; set; }
        public virtual DbSet<Loan_Account_Info> Loan_Account_Info { get; set; }
        public virtual DbSet<Loan_IAS39_Info> Loan_IAS39_Info { get; set; }
        public virtual DbSet<Moody_LGD_Info> Moody_LGD_Info { get; set; }
        public virtual DbSet<Treasury_Securities_Info> Treasury_Securities_Info { get; set; }
        public virtual DbSet<Transfer_CheckTable> Transfer_CheckTable { get; set; }
        public virtual DbSet<Group_Product> Group_Product { get; set; }
        public virtual DbSet<Group_Product_Code_Mapping> Group_Product_Code_Mapping { get; set; }
        public virtual DbSet<Flow_Info> Flow_Info { get; set; }
        public virtual DbSet<EL_Data_Out> EL_Data_Out { get; set; }
        public virtual DbSet<Bond_Account_Info> Bond_Account_Info { get; set; }
        public virtual DbSet<Bond_Rating_Info> Bond_Rating_Info { get; set; }
        public virtual DbSet<Bond_Rating_Summary> Bond_Rating_Summary { get; set; }
        public virtual DbSet<Rating_History> Rating_History { get; set; }
        public virtual DbSet<EL_Data_In> EL_Data_In { get; set; }
        public virtual DbSet<IFRS9_Main> IFRS9_Main { get; set; }
    }
}
