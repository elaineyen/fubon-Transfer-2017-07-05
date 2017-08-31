using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using Transfer.Models.Interface;
using Transfer.Utility;
using Transfer.ViewModels;
using static Transfer.Enum.Ref;

namespace Transfer.Models.Repository
{
    public class A4Repository : IA4Repository, IDbEvent
    {
        #region 其他

        public A4Repository()
        {
            this.db = new IFRS9Entities();
            this.common = new Common();
        }

        protected IFRS9Entities db
        {
            get;
            private set;
        }

        protected Common common
        {
            get;
            private set;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void SaveChange()
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.db != null)
                {
                    this.db.Dispose();
                    this.db = null;
                }
            }
        }

        #endregion 其他

        #region Get Data

        /// <summary>
        /// get A41 data
        /// </summary>
        /// <param name="type">Report = 報導日資料查詢,Bonds = 債券資料查詢</param>
        /// <param name="value">版本 or 債券編號</param>
        /// <param name="date">報導日 or 債券購入日期</param>
        /// <returns></returns>
        public Tuple<bool, List<A41ViewModel>> GetA41(string type, string value, DateTime date)
        {
            if (db.Bond_Account_Info.Any())
            {
                if ("Report".Equals(type)) //報導日資料查詢
                {
                    int version = 0;
                    Int32.TryParse(value, out version);
                    var data = (from q in db.Bond_Account_Info
                                            .Where(x => date == x.Report_Date &&
                                             x.Version != null &&
                                             x.Version == version).AsEnumerable()
                                            .OrderBy(x => Convert.ToInt32(x.Reference_Nbr))
                                select DbToA41Model(q)).ToList();
                    return new Tuple<bool, List<A41ViewModel>>(data.Any(), data);
                }
                if ("Bonds".Equals(type)) //債券資料查詢
                {
                    var data =
                     (from q in db.Bond_Account_Info
                                  .Where(x => date == x.Origination_Date &&
                                  value.Trim().Equals(x.Bond_Number)).AsEnumerable()
                                  .OrderBy(x => Convert.ToInt32(x.Reference_Nbr))
                      select DbToA41Model(q)).ToList();
                    return new Tuple<bool, List<A41ViewModel>>(data.Any(), data);
                }
            }
            return new Tuple<bool, List<A41ViewModel>>(false, new List<A41ViewModel>());
        }

        #endregion Get Data

        #region GetLogData

        /// <summary>
        /// get IFRS9_data
        /// </summary>
        /// <param name="tableTypes"></param>
        /// <param name="debt"></param>
        /// <returns></returns>
        public List<string> GetLogData(List<string> tableTypes, string debt)
        {
            List<string> result = new List<string>();
            try
            {
                if (db.IFRS9_Log.Any() && tableTypes.Any())
                {
                    foreach (string tableType in tableTypes)
                    {
                        var items = db.IFRS9_Log
                             //.AsEnumerable()
                             .Where(x => tableType.Equals(x.Table_type) &&
                             debt.Equals(x.Debt_Type)).ToList();
                        if (items.Any())
                        {
                            var lastDate = items.Max(y => y.Create_date);
                            result.AddRange(items.Where(x => lastDate.Equals(x.Create_date))
                                .OrderByDescending(x => x.Create_time)
                                .Select(x =>
                                {
                                    return string.Format("{0} {1} {2} {3}",
                                      x.Table_type,
                                      x.Create_date,
                                      x.Create_time,
                                      "Y".Equals(x.TYPE) ? "成功" : "失敗"
                                      );
                                }));
                        }
                    }
                }
            }
            catch
            {
            }
            return result;
        }

        #endregion GetLogData

        #region Save DB 部分

        #region Save A41

        /// <summary>
        /// A41 save db
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        public MSGReturnModel saveA41(List<A41ViewModel> dataModel, string reportDate)
        {
            MSGReturnModel result = new MSGReturnModel();
            DateTime start = DateTime.Now;
            //DateTime dt = TypeTransfer.stringToDateTime(reportDate);
            string type = Table_Type.A41.ToString();

            if (!dataModel.Any())
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type.not_Find_Any.GetDescription();
                return result;
            }
            var dts = dataModel.Select(x => x.Report_Date).Distinct();
            var vers = dataModel.Select(x => x.Version).Distinct();
            if (dts.Count() > 1 || vers.Count() > 1)
            {
                //report Date 大於一種判斷要不要防呆
            }
            DateTime dt = TypeTransfer.stringToDateTime(dts.First());
            int verInt = 0;
            string ver = vers.First();
            if (!Int32.TryParse(ver, out verInt))
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type.parameter_Error.GetDescription();
                return result;
            }
                
            if (db.Bond_Account_Info.Any() &&
                (!common.checkTransferCheck(type, type, dt, verInt) ||
                 db.Bond_Account_Info
                .FirstOrDefault(x => x.Report_Date != null &&
                                      x.Report_Date == dt &&
                                      x.Version == verInt) != null))
            //資料裡面已經有相同的 Report_Date , version
            {
                common.saveTransferCheck(
                    type,
                    false,
                    dt,
                    verInt,
                    start,
                    DateTime.Now
                );
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type.already_Save.GetDescription();
                return result;
            }
            foreach (var item in dataModel)
            {
                db.Bond_Account_Info.Add(
                new Bond_Account_Info()
                {
                    Reference_Nbr = item.Reference_Nbr,
                    Bond_Number = item.Bond_Number,
                    Lots = item.Lots,
                    Segment_Name = item.Segment_Name,
                    CURR_SP_Issuer = item.Curr_Sp_Issuer,
                    CURR_Moodys_Issuer = item.Curr_Moodys_Issuer,
                    CURR_Fitch_Issuer = item.Curr_Fitch_Issuer,
                    CURR_TW_Issuer = item.Curr_Tw_Issuer,
                    CURR_SP_Issue = item.Curr_Sp_Issue,
                    CURR_Moodys_Issue = item.Curr_Moodys_Issue,
                    CURR_Fitch_Issue = item.Curr_Fitch_Issue,
                    CURR_TW_Issue = item.Curr_Tw_Issue,
                    Ori_Amount = TypeTransfer.stringToDoubleN(item.Ori_Amount),
                    Current_Int_Rate = TypeTransfer.stringToDoubleN(item.Current_Int_Rate),
                    Origination_Date = TypeTransfer.stringToDateTimeN(item.Origination_Date),
                    Maturity_Date = TypeTransfer.stringToDateTimeN(item.Maturity_Date),
                    Principal_Payment_Method_Code = item.Principal_Payment_Method_Code,
                    Payment_Frequency = item.Payment_Frequency,
                    Baloon_Freq = item.Baloon_Freq,
                    ISSUER_AREA = item.Issuer_Area,
                    Industry_Sector = item.Industry_Sector,
                    PRODUCT = item.Product,
                    FINANCE_INSTRUMENTS = item.Finance_Instruments,
                    IAS39_CATEGORY = item.Ias39_Category,
                    Principal = TypeTransfer.stringToDoubleN(item.Principal),
                    Amort_Amt_Tw = TypeTransfer.stringToDoubleN(item.Amort_Amt_Tw),
                    Interest_Receivable = TypeTransfer.stringToDoubleN(item.Interest_Receivable),
                    Interest_Receivable_tw = TypeTransfer.stringToDoubleN(item.Interest_Receivable_tw),
                    Interest_Rate_Type = item.Interest_Rate_Type,
                    IMPAIR_YN = item.Impair_Yn,
                    EIR = TypeTransfer.stringToDoubleN(item.Eir),
                    Currency_Code = item.Currency_Code,
                    Report_Date = TypeTransfer.stringToDateTimeN(item.Report_Date),
                    ISSUER = item.Issuer,
                    Country_Risk = item.Country_Risk,
                    Ex_rate = TypeTransfer.stringToDoubleN(item.Ex_rate),
                    Lien_position = item.Lien_position,
                    Portfolio = item.Portfolio,
                    //Dept = item.Dept,
                    ASSET_SEG = item.Asset_Seg,
                    Ori_Ex_rate = TypeTransfer.stringToDoubleN(item.Ori_Ex_rate),
                    Bond_Type = item.Bond_Type,
                    Assessment_Sub_Kind = item.Assessment_Sub_Kind,
                    Processing_Date = TypeTransfer.stringToDateTimeN(item.Processing_Date),
                    Version = TypeTransfer.stringToIntN(item.Version),
                    Bond_Aera = "NTD".Equals(item.Currency_Code) ? "國內" : "國外",
                    //IH_OS  //IH->自操，OS->委外
                    Amount_TW_Ori_Ex_rate = TypeTransfer.DoubleNMultip(
                        TypeTransfer.stringToDoubleN(item.Ori_Amount),
                        TypeTransfer.stringToDoubleN(item.Ori_Ex_rate)),
                    Amort_Amt_Ori_Tw = TypeTransfer.DoubleNMultip(
                        TypeTransfer.stringToDoubleN(item.Principal),
                        TypeTransfer.stringToDoubleN(item.Ori_Ex_rate)),
                    //Market_Value_Ori = //需為已乘上單位數的市價
                    //Market_Value_TW = //(57)Market_Value_Ori*(41)Ex_rate
                    //Value_date = 
                    //Portfolio_Name = 
                });
            }
            try
            {
                db.SaveChanges(); //Save
                common.saveTransferCheck(
                       type,
                       true,
                       dt,
                       verInt,
                       start,
                       DateTime.Now
                   );
                result.RETURN_FLAG = true;
            }
            catch (DbUpdateException ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type
                        .save_Fail.GetDescription(type,
                        $"message: {ex.Message}" +
                        $", inner message {ex.InnerException?.InnerException?.Message}");
                common.saveTransferCheck(
                        type,
                        false,
                        dt,
                        verInt,
                        start,
                        DateTime.Now
                    );
            }
            return result;
        }

        #endregion Save A41

        #region Save A42
        /// <summary>
        /// A42 save db
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        public MSGReturnModel saveA42(List<A42ViewModel> dataModel)
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                if (!dataModel.Any())
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = Message_Type.not_Find_Any.GetDescription();
                    return result;
                }

                foreach (var item in dataModel)
                {
                    var query = db.Treasury_Securities_Info.AsEnumerable()
                               .Where(x => x.Bond_Number == item.Bond_Number
                                           && x.Lots == item.Lots
                                           && x.Report_Date.ToString("yyyy/MM/dd") == item.Report_Date);

                    db.Treasury_Securities_Info.RemoveRange(query);
                }

                foreach (var item in dataModel)
                {
                    db.Treasury_Securities_Info.Add(
                    new Treasury_Securities_Info()
                    {
                        Bond_Number = item.Bond_Number,
                        Lots = item.Lots,
                        Segment_Name = item.Segment_Name,
                        Portfolio_Name = item.Portfolio_Name,
                        Bond_Value = TypeTransfer.stringToDouble(item.Bond_Value),
                        Ori_Amount = TypeTransfer.stringToDouble(item.Ori_Amount),
                        Principal = TypeTransfer.stringToDouble(item.Principal),
                        Amort_value = TypeTransfer.stringToDouble(item.Amort_value),
                        Processing_Date = TypeTransfer.stringToDateTime(item.Processing_Date),
                        Report_Date = TypeTransfer.stringToDateTime(item.Report_Date)
                    });
                }

                db.SaveChanges(); //Save
                result.RETURN_FLAG = true;
            }
            catch (DbUpdateException ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type
                        .save_Fail.GetDescription(Table_Type.A42.ToString(),
                        $"message: {ex.Message}" +
                        $", inner message {ex.InnerException?.InnerException?.Message}");
            }

            return result;
        }
        #endregion Save A42

        #region Save B01

        /// <summary>
        /// save B01
        /// </summary>
        /// <param name="version"></param>
        /// <param name="date">Report_Date</param>
        /// <param name="type">M = 房貸 ,B = 債券</param>
        /// <returns></returns>
        public MSGReturnModel saveB01(string version, DateTime date, string type)
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type
                    .not_Find_Any.GetDescription("B01");
                if (Debt_Type.B.ToString().Equals(type)) //債券
                {
                    if (db.Bond_Account_Info.Any())
                    {
                        List<Bond_Account_Info> addData = //這次要新增的資料
                            db.Bond_Account_Info
                            .Where(x => x.Report_Date != null &&
                            date == x.Report_Date.Value //抓取相同的Report_date
                            && version.Equals(x.Version)).ToList();  //抓取相同的 Verison
                        if (!addData.Any())
                        {
                            result.DESCRIPTION = Message_Type
                                .query_Not_Find.GetDescription(Table_Type.B01.ToString());
                            return result;
                        }

                        if (db.IFRS9_Main.Any())
                        {
                            List<string> B01Ids = new List<string>();
                            B01Ids.AddRange(db.IFRS9_Main.AsEnumerable()
                            .Select(x => x.Reference_Nbr).ToList()); //抓取 B01 Reference_Nbr
                            addData = addData.Where(x =>
                            !B01Ids.Contains(x.Reference_Nbr)).ToList(); //排除 save 重複資料
                        }

                        if (!addData.Any())
                        {
                            result.DESCRIPTION = Message_Type
                                .already_Save.GetDescription(Table_Type.B01.ToString());
                            return result;
                        }
                        db.IFRS9_Main.AddRange(
                           addData.Select(x =>
                           {
                               return new IFRS9_Main()
                               {
                                   Reference_Nbr = x.Reference_Nbr, //
                                   //Customer_Nbr = //
                                   //Ead = //
                                   Principal = x.Principal,
                                   Interest_Receivable = x.Interest_Receivable, //
                                   Principal_Payment_Method_Code = x.Principal_Payment_Method_Code, //
                                   //Total_Period = //
                                   Current_Int_Rate = transferCurrentIntRate(x.Current_Int_Rate, x.EIR),//
                                   //Current_Pd = //
                                   //Current_Lgd = //
                                   //Remaining_Month = //
                                   Eir = TypeTransfer.doubleNToDouble(x.EIR) <= 0d ?
                                          0.00001 : x.EIR.Value / 100, //
                                   //CPD_Segment_Code = //
                                   //Processing_Date = //
                                   Product_Code = transferProductCode(x.Principal_Payment_Method_Code), //
                                   //Department = //
                                   //PD_Model_Code = //
                                   //18 //Current_Rating_Code = x.
                                   Report_Date = x.Report_Date.Value,
                                   Maturity_Date = x.Maturity_Date, //
                                   //Account_Code = //
                                   //BadCredit_Ind = //
                                   //Charge_Off_Ind = //
                                   //Collateral_Legal_Action_Ind = //
                                   //Credit_Black_List_Ind = //
                                   //Credit_Card_Block_code = //
                                   //Credit_Review_Risk_Grade = //
                                   Current_External_Rating = string.Empty, //function 待code
                                   //Current_External_Rating_1 = //
                                   //Current_External_Rating_2 = //
                                   //Current_External_Rating_3 = //
                                   //Current_External_Rating_4 = //
                                   //Current_External_Rating_On_Missing = //
                                   //Current_Internal_Rating = //
                                   //Default_Ind = //
                                   //Early_Warning_Ind = //
                                   //Five_Types_Delinquent_Category = //
                                   //Ias39_Impaire_Ind = //
                                   //Ias39_Impaire_Desc = //
                                   //Industry_Average_Rating = //
                                   //Manual_Identified_Impaire_Stage_Code = //
                                   //Internal_Risk_Classification = //
                                   //Off_Bs_Item_Paid_Amt = //
                                   Original_External_Rating = string.Empty, //function 待code
                                   //Original_External_Rating_1 = //
                                   //Original_External_Rating_2 = //
                                   //Original_External_Rating_3 = //
                                   //Original_External_Rating_4 = //
                                   //Original_External_Rating_On_Missing = //
                                   //Original_Internal_Rating = //
                                   //Other_BadCredit_Ind = //
                                   //Other_Lending_Max_Delinquent_Days = //
                                   //Product_Average_Rating = //
                                   //Restructure_Ind = //
                                   //Ten_Types_Delinquent_Category = //
                                   //Watch_List_Ind = //
                                   //Write_Off_Ind = //
                                   Version = x.Version, //
                                   Lien_position = x.Lien_position, //
                                   Ori_Amount = x.Ori_Amount, //
                                   Payment_Frequency = transferPaymentFrequency(x.Payment_Frequency, type)
                               };
                           }));
                        db.SaveChanges();
                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = Message_Type
                            .save_Success.GetDescription(Table_Type.B01.ToString());
                    }
                }
                if (Debt_Type.M.ToString().Equals(type)) //房貸
                {
                    if (db.Loan_IAS39_Info.Any() && db.Loan_Account_Info.Any())
                    {
                        List<Loan_IAS39_Info> IAS39Data =
                            db.Loan_IAS39_Info
                            .Where(x => date == x.Report_Date).ToList();
                        var ref_Nbrs = IAS39Data.Select(y => y.Reference_Nbr);
                        List<Loan_Account_Info> AccountData =
                            db.Loan_Account_Info.Where(x =>
                            ref_Nbrs.Contains(x.Reference_Nbr)).ToList();

                        if (IAS39Data.Any())
                        {
                            db.IFRS9_Main.AddRange(
                                IAS39Data.Select(x =>
                                {
                                    return new IFRS9_Main()
                                    {
                                        Reference_Nbr = x.Reference_Nbr,//,
                                        //Customer_Nbr = //
                                        //Ead = //
                                        Principal = x.Principal,
                                        Interest_Receivable = x.Interest_Receivable,
                                        //Principal_Payment_Method_Code =
                                        //Total_Period =
                                        Current_Int_Rate =
                                        transferCurrentIntRate(
                                        getLoanAccountInfo(AccountData, x.Reference_Nbr).Current_Int_Rate,
                                        x.EIR),
                                        //Current_Pd
                                        Current_Lgd = getLoanAccountInfo(AccountData, x.Reference_Nbr).Current_Lgd, //?
                                        //Remaining_Month
                                        Eir = x.EIR <= 0d ? 0.00001 : x.EIR / 100, //
                                        //CPD_Segment_Code
                                        //Processing_Date
                                        Product_Code = Product_Code.M.GetDescription(),
                                        //Department
                                        //PD_Model_Code
                                        Current_Rating_Code = getLoanAccountInfo(AccountData, x.Reference_Nbr).Current_Rating_Code,
                                        Report_Date = date,
                                        Maturity_Date = TypeTransfer.stringToADDateTimeN(
                                            getLoanAccountInfo(AccountData, x.Reference_Nbr).Lexp_Date),
                                        //Account_Code
                                        //BadCredit_Ind
                                        //Charge_Off_Ind
                                        Collateral_Legal_Action_Ind = x.Collateral_Legal_Action_Ind,
                                        //Credit_Black_List_Ind = //
                                        //Credit_Card_Block_code = //
                                        //Credit_Review_Risk_Grade = //
                                        //Current_External_Rating = string.Empty, //function 待code
                                        //Current_External_Rating_1 = //
                                        //Current_External_Rating_2 = //
                                        //Current_External_Rating_3 = //
                                        //Current_External_Rating_4 = //
                                        //Current_External_Rating_On_Missing = //
                                        //Current_Internal_Rating = //
                                        //Default_Ind = //
                                        Delinquent_Days = getLoanAccountInfo(AccountData, x.Reference_Nbr).Delinquent_Days,
                                        //Early_Warning_Ind
                                        //Five_Types_Delinquent_Category
                                        Ias39_Impaire_Ind = x.IAS39_Impaire_Ind,
                                        Ias39_Impaire_Desc = x.IAS39_Impaire_Desc,
                                        //Industry_Average_Rating = //
                                        //Manual_Identified_Impaire_Stage_Code = //
                                        //Internal_Risk_Classification = //
                                        //Off_Bs_Item_Paid_Amt = //
                                        //Original_External_Rating = string.Empty, //function 待code
                                        //Original_External_Rating_1 = //
                                        //Original_External_Rating_2 = //
                                        //Original_External_Rating_3 = //
                                        //Original_External_Rating_4 = //
                                        //Original_External_Rating_On_Missing = //
                                        //Original_Internal_Rating = //
                                        //Other_BadCredit_Ind = //
                                        //Other_Lending_Max_Delinquent_Days = //
                                        //Product_Average_Rating = //
                                        Restructure_Ind = x.Restructure_Ind,
                                        //Ten_Types_Delinquent_Category
                                        //Watch_List_Ind
                                        //Write_Off_Ind
                                        //Version
                                        //Lien_position
                                        //Ori_Amount
                                        //Payment_Frequency = getLoanAccountInfo(AccountData, x.Reference_Nbr).p
                                    };
                                }));
                            db.SaveChanges();
                            result.RETURN_FLAG = true;
                            result.DESCRIPTION = Message_Type
                                .save_Success.GetDescription(Table_Type.B01.ToString());
                        }
                    }
                }
            }
            catch (DbUpdateException ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type
                        .save_Fail.GetDescription(Table_Type.B01.ToString(),
                        $"message: {ex.Message}" +
                        $", inner message {ex.InnerException?.InnerException?.Message}");
            }
            return result;
        }

        #endregion Save B01

        #region Save C01

        /// <summary>
        /// Save C01
        /// </summary>
        /// <param name="version"></param>
        /// <param name="date">Report_Date</param>
        /// <param name="type">M = 房貸 ,B = 債券</param>
        /// <returns></returns>
        public MSGReturnModel saveC01(string version, DateTime date, string type)
        {
            MSGReturnModel result = new MSGReturnModel();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Message_Type
                    .not_Find_Any.GetDescription(Table_Type.C01.ToString());
            try
            {
                if (db.IFRS9_Main.Any())
                {
                    List<string> reportCodes = new List<string>();
                    if (Debt_Type.M.ToString().Equals(type)) //房貸
                        reportCodes.Add(Product_Code.M.GetDescription());
                    if (Debt_Type.B.ToString().Equals(type)) //債券
                        reportCodes = new List<string> { "Bond_A", "Bond_B", "Bond_P" };
                    List<IFRS9_Main> addData = //這次要新增的資料
                    db.IFRS9_Main
                    //.AsEnumerable()
                    .Where(x => x.Report_Date != null &&
                    date == x.Report_Date  //抓取相同的Report_date
                    //&& version.Equals(x.Version) //抓取相同的 Verison
                    && reportCodes.Contains(x.Product_Code)).ToList();  //抓取符合的 Product_Code

                    if (!addData.Any())
                    {
                        result.DESCRIPTION = Message_Type
                            .query_Not_Find.GetDescription(Table_Type.C01.ToString());
                        return result;
                    }

                    if (db.EL_Data_In.Any())
                    {
                        List<string> C01Ids = new List<string>();
                        C01Ids.AddRange(db.EL_Data_In
                        //.AsEnumerable()
                        .Select(x => x.Reference_Nbr).ToList()); //抓取 C01 Reference_Nbr
                        addData = addData.Where(x =>
                        !C01Ids.Contains(x.Reference_Nbr)).ToList(); //排除 save 重複資料
                    }

                    if (!addData.Any())
                    {
                        result.DESCRIPTION = Message_Type
                            .already_Save.GetDescription(Table_Type.C01.ToString());
                        return result;
                    }
                    DateTime now = DateTime.Now;

                    if (Debt_Type.M.ToString().Equals(type)) //房貸
                    {
                        db.EL_Data_In.AddRange(
                           addData.Select(x => new EL_Data_In()
                           {
                               Report_Date = x.Report_Date, //評估基準日/報導日
                               Processing_Date = now.Date, //資料處理日期
                               Product_Code = x.Product_Code, //產品
                               Reference_Nbr = x.Reference_Nbr, //案件編號/帳號
                               Current_Rating_Code = x.Current_Rating_Code, //風險區隔
                               Exposure = TypeTransfer.doubleNToDouble(x.Principal)
                               + TypeTransfer.doubleNToDouble(x.Interest_Receivable),
                               // 曝險額 = 餘額+利息 (4)Principal + (5)Interest_Receivable
                               Actual_Year_To_Maturity = x.Maturity_Date.dateSubtractToYear(x.Report_Date),
                               // 合約到期年限 = 由(到期日-評估基準日/報導日),取date再轉換成年
                               Duration_Year = x.Remaining_Month != null ?
                               getC01DurationYear(x.Remaining_Month) : x.Maturity_Date.dateSubtractToYear(x.Report_Date),
                               //估計存續期間_年
                               Remaining_Month = x.Remaining_Month != null ?
                               x.Remaining_Month : x.Maturity_Date.dateSubtractToMonths(x.Report_Date),
                               //估計存續期間_月
                               Current_LGD = TypeTransfer.doubleNToDouble(x.Current_Lgd) > 1 ? 1 :
                               (TypeTransfer.doubleNToDouble(x.Current_Lgd) < 0 ? 0 :
                               TypeTransfer.doubleNToDouble(x.Current_Lgd)),
                               //違約損失率    若> 1 給值1, 若小於0,給值0
                               Current_Int_Rate = x.Current_Int_Rate, //合约利率/產品利率
                               EIR = x.Eir, //有效利率
                               Impairment_Stage = getMortgageC01ImpairmentStage(
                                   x.Collateral_Legal_Action_Ind,
                                   x.Delinquent_Days,
                                   x.Ias39_Impaire_Ind,
                                   x.Ias39_Impaire_Desc,
                                   x.Restructure_Ind
                                   ), //減損階段
                               //Version = x.Version, //資料版本
                               //Lien_position = x.Lien_position, //擔保順位
                               //Ori_Amount = x.Ori_Amount, //原始購買金額
                               //Principal = x.Principal, //金融資產餘額
                               //Interest_Receivable = x.Interest_Receivable, //應收利息
                               Payment_Frequency = x.Payment_Frequency //償還(繳款)頻率 (次/年)
                           }));
                    }
                    if (Debt_Type.B.ToString().Equals(type)) //債券
                    {
                        db.EL_Data_In.AddRange(
                           addData.Select(x => new EL_Data_In()
                           {
                               Report_Date = x.Report_Date, //評估基準日/報導日
                               Processing_Date = now.Date, //資料處理日期
                               Product_Code = x.Product_Code, //產品
                               Reference_Nbr = x.Reference_Nbr, //案件編號/帳號
                               Current_Rating_Code = x.Current_Rating_Code, //風險區隔
                               Exposure = TypeTransfer.doubleNToDouble(x.Principal)
                               + TypeTransfer.doubleNToDouble(x.Interest_Receivable),
                               // 曝險額 = 餘額+利息 (4)Principal + (5)Interest_Receivable
                               Actual_Year_To_Maturity = x.Maturity_Date.dateSubtractToYear(x.Report_Date),
                               // 合約到期年限 = 由(到期日-評估基準日/報導日),取date再轉換成年
                               Duration_Year = x.Remaining_Month != null ?
                               getC01DurationYear(x.Remaining_Month) : x.Maturity_Date.dateSubtractToYear(x.Report_Date),
                               //估計存續期間_年
                               Remaining_Month = x.Remaining_Month != null ?
                               x.Remaining_Month : x.Maturity_Date.dateSubtractToMonths(x.Report_Date),
                               //估計存續期間_月
                               Current_LGD = TypeTransfer.doubleNToDouble(x.Current_Lgd) > 1 ? 1 :
                               (TypeTransfer.doubleNToDouble(x.Current_Lgd) < 0 ? 0 :
                               TypeTransfer.doubleNToDouble(x.Current_Lgd)),
                               //違約損失率    若> 1 給值1, 若小於0,給值0
                               Current_Int_Rate = x.Current_Int_Rate, //合约利率/產品利率
                               EIR = x.Eir, //有效利率
                               Impairment_Stage = getBondsC01ImpairmentStage(x.Product_Code), //減損階段
                               Version = x.Version, //資料版本
                               Lien_position = x.Lien_position, //擔保順位
                               Ori_Amount = x.Ori_Amount, //原始購買金額
                               Principal = x.Principal, //金融資產餘額
                               Interest_Receivable = x.Interest_Receivable, //應收利息
                               Payment_Frequency = x.Payment_Frequency //償還(繳款)頻率 (次/年)
                           }));
                    }
                    db.SaveChanges();
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Message_Type
                        .save_Success.GetDescription(Table_Type.C01.ToString());
                }
            }
            catch (DbUpdateException ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type
                        .save_Fail.GetDescription(Table_Type.C01.ToString(),
                        $"message: {ex.Message}" +
                        $", inner message {ex.InnerException?.InnerException?.Message}");
            }
            return result;
        }

        #endregion Save C01

        #region Save C02
        /// <summary>
        /// Save C02
        /// </summary>
        /// <param name="version"></param>
        /// <param name="date">Report_Date</param>
        /// <param name="type">M = 房貸</param>
        /// <returns></returns>
        public MSGReturnModel saveC02(string version, DateTime date, string type)
        {
            MSGReturnModel result = new MSGReturnModel();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Message_Type
                    .not_Find_Any.GetDescription(Table_Type.C02.ToString());
            try
            {
                if (db.Loan_Account_Info.Any())
                {
                    List<string> reportCodes = new List<string>();

                    if (Debt_Type.M.ToString().Equals(type)) //房貸
                    {
                        string productCode = GroupProductCode.M.GetDescription();
                        productCode = db.Group_Product_Code_Mapping.Where(x => x.Group_Product_Code.StartsWith(productCode)).FirstOrDefault().Product_Code;

                        reportCodes.Add(productCode);
                    }

                    List<Loan_Account_Info> addData = //這次要新增的資料
                    db.Loan_Account_Info.AsEnumerable()
                    .Where(x => x.Report_Date != null
                                && x.Report_Date >= date.AddMonths(-13)
                                && x.Report_Date <= date).ToList();

                    if (!addData.Any())
                    {
                        result.DESCRIPTION = Message_Type
                            .query_Not_Find.GetDescription(Table_Type.C02.ToString());

                        return result;
                    }

                    if (db.Rating_History.Any())
                    {
                        foreach (var item in addData)
                        {
                            var query = db.Rating_History
                                       .Where(x => x.Reference_Nbr == item.Reference_Nbr
                                                   && x.Rating_Date == item.Rating_Date);

                            db.Rating_History.RemoveRange(query);
                        }
                    }

                    DateTime now = DateTime.Now;

                    if (Debt_Type.M.ToString().Equals(type)) //房貸
                    {
                        db.Rating_History.AddRange(
                           addData.Select(x => new Rating_History()
                           {
                               Data_ID = "",
                               Processing_Date = now.Date.ToString("yyyy/MM/dd"), //資料處理日期
                               Product_Code = reportCodes[0], //產品
                               Reference_Nbr = x.Reference_Nbr, //案件編號/帳號
                               Current_Rating_Code = x.Current_Rating_Code, //月評等等級
                               Rating_Date = x.Rating_Date
                           }));
                    }

                    db.SaveChanges();
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Message_Type
                        .save_Success.GetDescription(Table_Type.C02.ToString());
                }
            }
            catch (DbUpdateException ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type
                        .save_Fail.GetDescription(Table_Type.C02.ToString(),
                        $"message: {ex.Message}" +
                        $", inner message {ex.InnerException?.InnerException?.Message}");
            }

            return result;
        }
        #endregion Save C02

        #endregion Save DB 部分

        #region Excel 部分

        #region Excel 資料轉成 A41ViewModel

        /// <summary>
        /// Excel 資料轉成 A41ViewModel
        /// </summary>
        /// <param name="pathType">Excel 副檔名</param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public List<A41ViewModel> getExcel(string pathType, Stream stream)
        {
            DataSet resultData = new DataSet();
            List<A41ViewModel> dataModel = new List<A41ViewModel>();
            try
            {
                IExcelDataReader reader = null;
                switch (pathType) //判斷型別
                {
                    case "xls":
                        reader = ExcelReaderFactory.CreateBinaryReader(stream);
                        break;

                    case "xlsx":
                        reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                        break;
                }
                reader.IsFirstRowAsColumnNames = true;
                resultData = reader.AsDataSet();
                reader.Close();
                int idNum = 0;
                if (db.Bond_Account_Info.Any())
                    idNum = db.Bond_Account_Info
                        .Select(x => x.Reference_Nbr).Distinct().AsEnumerable()
                        .Max(x => Convert.ToInt32(x));
                if (resultData.Tables[1].Rows.Count > 2) //判斷有無資料
                {
                    dataModel = resultData.Tables[1].AsEnumerable().Skip(1) //第二頁籤第二行開始
                        .Select((x, y) =>
                        {
                            return getA41Model(x, (y + 1 + idNum).ToString().PadLeft(10, '0'));
                        }
                        ).ToList();

                    //skip(1) 為排除第一行 Excel Title列那行(參數可調)
                }
            }
            catch (Exception ex)
            { }
            return dataModel;
        }

        #endregion Excel 資料轉成 A41ViewModel

        #region Excel 資料轉成 A42ViewModel

        /// <summary>
        /// Excel 資料轉成 A42ViewModel
        /// </summary>
        /// <param name="pathType">Excel 副檔名</param>
        /// <param name="stream"></param>
        /// <param name="reportDate"></param>
        /// <returns></returns>
        public List<A42ViewModel> getA42Excel(string pathType, Stream stream, string processingDate, string reportDate)
        {
            DataSet resultData = new DataSet();
            List<A42ViewModel> dataModel = new List<A42ViewModel>();

            try
            {
                IExcelDataReader reader = null;
                switch (pathType) //判斷型別
                {
                    case "xls":
                        reader = ExcelReaderFactory.CreateBinaryReader(stream);
                        break;

                    case "xlsx":
                        reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                        break;
                }

                reader.IsFirstRowAsColumnNames = true;
                resultData = reader.AsDataSet();
                reader.Close();

                if (resultData.Tables[0].Rows.Count > 1) //判斷有無資料
                {
                    dataModel = (from q in resultData.Tables[0].AsEnumerable()
                                 select getA42ViewModel(q, processingDate, reportDate)).ToList();
                }
            }
            catch (Exception ex)
            {
            }

            return dataModel;
        }

        #endregion Excel 資料轉成 A42ViewModel

        #endregion Excel 部分

        #region Private Function

        #region datarow 組成 A41ViewModel

        /// <summary>
        /// datarow 組成 A41ViewModel
        /// </summary>
        /// <param name="item">DataRow</param>
        /// <returns>A41ViewModel</returns>
        private A41ViewModel getA41Model(DataRow item, string num)
        {
            var Maturity_Date = TypeTransfer.objToString(item[14]);
            DateTime MDate = DateTime.MinValue;
            var Balloon_Date = TypeTransfer.objToString(item[16]);
            string Principal_Payment_Method_Code = "01";
            // Balloon_Date IN ('0') OR  Balloon_Date IS NULL THEN '02'
            if (Balloon_Date.IsNullOrWhiteSpace() || "0".Equals(Balloon_Date))
                Principal_Payment_Method_Code = "02";
            //Year( Maturity_Date) > = 2100  Then  '04'
            if (DateTime.TryParse(Maturity_Date, out MDate) && MDate.Year > 2100)
                Principal_Payment_Method_Code = "04";
            return new A41ViewModel()
            {
                Reference_Nbr = num, //帳戶編號/群組編號
                Bond_Number = TypeTransfer.objToString(item[0]), //A 債券編號
                Lots = TypeTransfer.objToString(item[1]), //B Lots
                Segment_Name = TypeTransfer.objToString(item[2]), //C 債券(資產)名稱
                Curr_Sp_Issuer = TypeTransfer.objToString(item[3]), //D 最近發行人評等_SP
                Curr_Moodys_Issuer = TypeTransfer.objToString(item[4]), //E 最近發行人評等_Moody
                Curr_Fitch_Issuer = TypeTransfer.objToString(item[5]), //F 最近發行人評等_Fitch
                Curr_Tw_Issuer = TypeTransfer.objToString(item[6]), //G 最近發行人評等_中華
                Curr_Sp_Issue = TypeTransfer.objToString(item[7]), //H 最近債項評等_SP
                Curr_Moodys_Issue = TypeTransfer.objToString(item[8]), //I 最近債項評等_Moody
                Curr_Fitch_Issue = TypeTransfer.objToString(item[9]), //J 最近債項評等_Fitch
                Curr_Tw_Issue = TypeTransfer.objToString(item[10]), //K 最近債項評等_中華
                Ori_Amount = TypeTransfer.objToString(item[11]), //L 原始金額
                Current_Int_Rate = TypeTransfer.objToString(item[12]), //M 合約利率
                Origination_Date = TypeTransfer.objDateToString(item[13]), //N 債券購入(認列)日期
                Maturity_Date = TypeTransfer.objDateToString(item[14]), //O (缺 P=>15) 到期日
                Principal_Payment_Method_Code = Principal_Payment_Method_Code,
                Payment_Frequency = TypeTransfer.objDateToString(item[16]), //Q 票面利率週期
                Baloon_Freq = TypeTransfer.objToString(item[17]), //R (缺 S=>18) 贖回日期(本金一次贖回)
                Issuer_Area = TypeTransfer.objToString(item[19]), //T Issuer所屬區域
                Industry_Sector = TypeTransfer.objToString(item[20]), //U 對手產業別
                Product = TypeTransfer.objToString(item[21]), //V 債券產品別(揭露使用)
                Finance_Instruments = TypeTransfer.objToString(item[22]), //W (缺 X=>23) 金融工具類型(金融資產/放款/金融保證)
                Ias39_Category = TypeTransfer.objToString(item[24]), //Y 原公報分類
                Principal = TypeTransfer.objToString(item[25]), //Z 金融資產餘額 攤銷後之成本數(原幣)
                Amort_Amt_Tw = TypeTransfer.objToString(item[26]), //AA 金融資產餘額(台幣) 攤銷後之成本數(台幣)
                Interest_Receivable = TypeTransfer.objToString(item[27]), //AB 應收利息(原幣)
                Interest_Receivable_tw = TypeTransfer.objToString(item[28]), //AC (缺 AD=>29) 應收利息(台幣)
                Interest_Rate_Type = string.Empty,
                Impair_Yn = TypeTransfer.objToString(item[30]), //AE 是否有客觀減損證據
                Eir = TypeTransfer.objToString(item[31]), //AF 有效利率(折現率)
                Currency_Code = TypeTransfer.objToString(item[32]), //AG 債券幣別
                Report_Date = TypeTransfer.objDateToString(item[33]), //AH 評估基準日/報導日
                Issuer = TypeTransfer.objToString(item[34]), //AI 發行人
                Country_Risk = TypeTransfer.objToString(item[35]), //AJ 保證國別
                Ex_rate = TypeTransfer.objToString(item[36]), //AK 月底匯率
                Lien_position = TypeTransfer.objToString(item[37]), //AL 債券擔保順位
                Portfolio = TypeTransfer.objToString(item[38]), //AM 資產組合名稱
                Dept = TypeTransfer.objToString(item[39]), //AN 部門
                Asset_Seg = TypeTransfer.objToString(item[40]), //AO 資產區隔
                Ori_Ex_rate = TypeTransfer.objToString(item[41]), //AP 成本匯率
                Bond_Type = TypeTransfer.objToString(item[42]), //AQ 用來判斷該債券為公債/國營事業債，或屬其餘債券
                Assessment_Sub_Kind = TypeTransfer.objToString(item[43]), //AR 產業公司/金融債主順位債券/金融債次順位債券
                Processing_Date = TypeTransfer.objToString(item[44]), //AS 資料處理日期
                Version = TypeTransfer.objToString(item[45]) //AT 資料版本
            };
        }

        #endregion datarow 組成 A41ViewModel

        #region Db 組成 A41ViewModel

        /// <summary>
        /// Db 組成 A41ViewModel
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private A41ViewModel DbToA41Model(Bond_Account_Info data)
        {
            return new A41ViewModel()
            {
                Reference_Nbr = data.Reference_Nbr.PadLeft(10, '0'),
                Bond_Number = data.Bond_Number,
                Lots = data.Lots,
                Segment_Name = data.Segment_Name,
                Curr_Sp_Issuer = data.CURR_SP_Issuer,
                Curr_Moodys_Issuer = data.CURR_Moodys_Issuer,
                Curr_Fitch_Issuer = data.CURR_Fitch_Issuer,
                Curr_Tw_Issuer = data.CURR_TW_Issuer,
                Curr_Sp_Issue = data.CURR_SP_Issue,
                Curr_Moodys_Issue = data.CURR_Moodys_Issue,
                Curr_Fitch_Issue = data.CURR_Fitch_Issue,
                Curr_Tw_Issue = data.CURR_TW_Issue,
                Ori_Amount = TypeTransfer.doubleNToString(data.Ori_Amount),
                Current_Int_Rate = TypeTransfer.doubleNToString(data.Current_Int_Rate),
                Origination_Date = TypeTransfer.dateTimeNToString(data.Origination_Date),
                Maturity_Date = TypeTransfer.dateTimeNToString(data.Maturity_Date),
                Principal_Payment_Method_Code = data.Principal_Payment_Method_Code,
                Payment_Frequency = data.Payment_Frequency,
                Baloon_Freq = data.Baloon_Freq,
                Issuer_Area = data.ISSUER_AREA,
                Industry_Sector = data.Industry_Sector,
                Product = data.PRODUCT,
                Finance_Instruments = data.FINANCE_INSTRUMENTS,
                Ias39_Category = data.IAS39_CATEGORY,
                Principal = TypeTransfer.doubleNToString(data.Principal),
                Amort_Amt_Tw = TypeTransfer.doubleNToString(data.Amort_Amt_Tw),
                Interest_Receivable = TypeTransfer.doubleNToString(data.Interest_Receivable),
                Interest_Receivable_tw = TypeTransfer.doubleNToString(data.Interest_Receivable_tw),
                Interest_Rate_Type = data.Interest_Rate_Type,
                Impair_Yn = data.IMPAIR_YN,
                Eir = TypeTransfer.doubleNToString(data.EIR),
                Currency_Code = data.Currency_Code,
                Report_Date = TypeTransfer.dateTimeNToString(data.Report_Date),
                Issuer = data.ISSUER,
                Country_Risk = data.Country_Risk,
                Ex_rate = TypeTransfer.doubleNToString(data.Ex_rate),
                Lien_position = data.Lien_position,
                Portfolio = data.Portfolio,
                //Dept = data.Dept,
                Asset_Seg = data.ASSET_SEG,
                Ori_Ex_rate = TypeTransfer.doubleNToString(data.Ori_Ex_rate),
                Bond_Type = data.Bond_Type,
                Assessment_Sub_Kind = data.Assessment_Sub_Kind,
                Processing_Date = TypeTransfer.dateTimeNToString(data.Processing_Date),
                Version = TypeTransfer.intNToString(data.Version),
                Bond_Aera = data.Bond_Aera,
                Asset_Type = data.Asset_Type,
                IH_OS = data.IH_OS,
                Amount_TW_Ori_Ex_rate = TypeTransfer.doubleNToString(data.Amount_TW_Ori_Ex_rate),
                Amort_Amt_Ori_Tw = TypeTransfer.doubleNToString(data.Amort_Amt_Ori_Tw),
                Market_Value_Ori = TypeTransfer.doubleNToString(data.Market_Value_Ori),
                Market_Value_TW = TypeTransfer.doubleNToString(data.Market_Value_TW),
                Value_date = TypeTransfer.dateTimeNToString(data.Value_date)
            };
        }

        #endregion Db 組成 A41ViewModel

        #region datarow 組成 A42ViewModel

        /// <summary>
        /// datarow 組成 A42ViewModel
        /// </summary>
        /// <param name="item">DataRow</param>
        /// <returns>A42ViewModel</returns>
        private A42ViewModel getA42ViewModel(DataRow item, string processingDate, string reportDate)
        {
            return new A42ViewModel()
            {
                Bond_Number = TypeTransfer.objToString(item[0]),
                Lots = TypeTransfer.objToString(item[1]),
                Segment_Name = TypeTransfer.objToString(item[2]),
                Portfolio_Name = TypeTransfer.objToString(item[3]),
                Bond_Value = TypeTransfer.objToString(item[4]),
                Ori_Amount = TypeTransfer.objToString(item[5]),
                Principal = TypeTransfer.objToString(item[6]),
                Amort_value = TypeTransfer.objToString(item[7]),
                Processing_Date = processingDate,
                Report_Date = reportDate
            };
        }

        #endregion datarow 組成 A42ViewModel

        #region get 債券 B01 Product_Code

        /// <summary>
        /// get 債券 B01 Product_Code
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string transferProductCode(string value)
        {
            if (value.IsNullOrWhiteSpace())
                return value;

            switch (value)
            {
                case "01":
                    return Product_Code.B_A.GetDescription();

                case "02":
                    return Product_Code.B_B.GetDescription();

                case "04":
                    return Product_Code.B_P.GetDescription();
            }
            return value;
        }

        #endregion get 債券 B01 Product_Code

        #region get Current_Int_Rate To B01

        /// <summary>
        /// get Current_Int_Rate To B01
        /// </summary>
        /// <param name="currentIntRate"></param>
        /// <param name="Eir"></param>
        /// <returns></returns>
        private double? transferCurrentIntRate(double? currentIntRate, double? Eir)
        {
            if (0d == TypeTransfer.doubleNToDouble(currentIntRate))
            {
                //CASE WHEN( Current_Int_Rate =0 OR Current_Int_Rate IS NULL) AND A01.EIR=0 THEN 0.000001
                if (0d == TypeTransfer.doubleNToDouble(Eir))
                    return 0.000001;
                //WHEN ( Current_Int_Rate =0 OR Current_Int_Rate IS NULL) AND   A01.EIR<>0 THEN A01.EIR/100
                else
                    return Eir.Value / 100;
            }
            //ELSE  Current_Int_Rate/100 END
            return currentIntRate.Value / 100;
        }

        #endregion get Current_Int_Rate To B01

        #region A41 PaymentFrequency To B01

        /// <summary>
        /// A41 PaymentFrequency To B01
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private int? transferPaymentFrequency(string value, string type)
        {
            if (value.IsNullOrWhiteSpace())
                return null;
            if (Debt_Type.M.ToString().Equals(type)) //房貸
            {
                switch (value)
                {
                    case "M":
                        return 12;
                }
                //int i = 0;
                //if (Int32.TryParse(value, out i))
                //    return i;
            }
            if (Debt_Type.B.ToString().Equals(type)) //債券
            {
                switch (value)
                {
                    case "A":
                        return 1;

                    case "S":
                        return 2;
                }
                int i = 0;
                if (Int32.TryParse(value, out i))
                    return i;
            }

            return null;
        }

        #endregion A41 PaymentFrequency To B01

        #region Get C01 Duration_Year

        /// <summary>
        /// C01 Duration_Year
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private double? getC01DurationYear(int? value)
        {
            return (value.Value / 12);
        }

        #endregion Get C01 Duration_Year

        #region Get 債券 C01 Impairment_Stage

        /// <summary>
        /// Get 債券 C01 Impairment_Stage
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string getBondsC01ImpairmentStage(string value)
        {
            List<string> pro_codes = new List<string>() { "LOAN_A", "LOAN_B", "LONE_P" };
            if (value.IsNullOrWhiteSpace())
                return null;
            return pro_codes.Contains(value) ? "1" : string.Empty;
        }

        #endregion Get 債券 C01 Impairment_Stage

        #region get Loan_Account_Info

        /// <summary>
        /// get Loan_Account_Info
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="Reference_Nbr"></param>
        /// <returns></returns>
        private Loan_Account_Info getLoanAccountInfo(
            List<Loan_Account_Info> datas, string Reference_Nbr)
        {
            Loan_Account_Info data = datas.FirstOrDefault(x =>
            Reference_Nbr.Equals(x.Reference_Nbr));
            if (data != null)
                return data;
            else
                return new Loan_Account_Info();
        }

        #endregion get Loan_Account_Info

        #region get 房貸 C01 Impairment_Stage

        /// <summary>
        /// get 房貸 C01 Impairment_Stage
        /// </summary>
        /// <param name="Collateral_Legal_Action_Ind"></param>
        /// <param name="Delinquent_Days"></param>
        /// <param name="Ias39_Impaire_Ind"></param>
        /// <param name="Ias39_Impaire_Desc"></param>
        /// <param name="Restructure_Ind"></param>
        /// <returns></returns>
        private string getMortgageC01ImpairmentStage(
            string Collateral_Legal_Action_Ind,
            int? Delinquent_Days,
            string Ias39_Impaire_Ind,
            string Ias39_Impaire_Desc,
            string Restructure_Ind
            )
        {
            //WHEN (IAS39_Impaire_Ind="Y"  AND  IAS39_Impaire_Desc <> "逾期 29天" ) THEN 3
            if ("Y".Equals(Ias39_Impaire_Ind) && !"逾期 29天".Equals(Ias39_Impaire_Desc))
                return "3";
            //WHEN Delinquent_Days>=100 THEN 3
            if (TypeTransfer.intNToInt(Delinquent_Days) >= 100)
                return "3";
            //WHEN Delinquent_Days IS NULL   THEN 1    /*帳卡上已結清*/
            if (!Delinquent_Days.HasValue)
                return "1";
            //WHEN Restructure_Ind =“Y” THEN 2   /* 紓困名單*/
            if ("Y".Equals(Restructure_Ind))
                return "2";
            //WHEN Collateral_Legal_Action_Ind =‘Y ’   THEN 2       /*假扣押名單*/
            if ("Y".Equals(Collateral_Legal_Action_Ind))
                return "2";
            int d = TypeTransfer.intNToInt(Delinquent_Days);
            //WHEN Delinquent_Days> 29   AND Delinquent_Days < 90  THEN 2   /*7/7 金控風控建議修改/
            if (d > 29 && d < 90)
                return "2";
            //WHEN Delinquent_Days<30 THEN 1                                 /*7/7 金控風控建議修改/
            if (d < 30)
                return "1";
            return null;
        }

        #endregion get 房貸 C01 Impairment_Stage

        #endregion Private Function
    }
}