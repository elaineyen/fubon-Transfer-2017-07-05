﻿using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using Transfer.Enum;
using Transfer.Models.Interface;
using Transfer.Utility;
using Transfer.ViewModels;

namespace Transfer.Models.Repositiry
{
    public class A4Repository : IA4Repository, IDbEvent
    {
        #region 其他
        protected IFRS9Entities db
        {
            get;
            private set;
        }

        public A4Repository()
        {
            this.db = new IFRS9Entities();
        }

        public void SaveChange()
        {
            throw new NotImplementedException();
        }
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
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
        #endregion

        #region Get Data
        /// <summary>
        /// get A41 data
        /// </summary>
        /// <returns></returns>
        public Tuple<bool, List<A41ViewModel>> GetA41()
        {
            if (db.Bond_Account_Info.Count() > 0)
            {
                return new Tuple<bool, List<A41ViewModel>>(true,
                    (from q in db.Bond_Account_Info.AsEnumerable()
                     select DbToA41Model(q)).ToList());
            }
            return new Tuple<bool, List<A41ViewModel>>(false, new List<A41ViewModel>());
        }
        #endregion

        #region 
        public List<string> GetLogData(List<string> tableTypes)
        {
            List<string> result = new List<string>();
            try
            {
                if (db.IFRS9_Log.Count() > 0 && tableTypes.Count > 0)
                {
                    foreach (string tableType in tableTypes)
                    {
                       var items = db.IFRS9_Log.AsEnumerable()
                            .Where(x => tableType.Equals(x.Table_type)).ToList();
                        if (items.Count > 0)
                        {
                            var lastDate = items.Max(y =>y.Create_date);
                            result.AddRange(items.Where(x => lastDate.Equals(x.Create_date))
                                .OrderByDescending(x => x.Create_time)
                                .Select(x => 
                                { return string.Format("{0} {1} {2} {3}",
                                    x.Table_type,
                                    x.Create_date,
                                    x.Create_time,
                                    "Y".Equals(x.TYPE) ? "成功":"失敗"
                                    );}));
                            
                        }

                    }
                }
            }
            catch
            {

            }
            return result;
        }

        #endregion

        #region Save DB 部分

        #region Save A41
        /// <summary>
        /// A41 save db
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        public MSGReturnModel saveA41(List<A41ViewModel> dataModel)
        {
            MSGReturnModel result = new MSGReturnModel();
            int now_id = 0;
            try
            {
                if (0.Equals(dataModel.Count))
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = "No Save Data!";
                    return result;
                }
                if (db.Bond_Account_Info.Count() > 0 &&
                    db.Bond_Account_Info.AsEnumerable().
                    FirstOrDefault(x => x.Report_Date != null
                    && x.Report_Date.Value.ToString("yyyy/MM/dd")
                    .Equals(dataModel.First().Report_Date)) != null)
                    //資料裡面已經有相同的 Report_Date ?? 是否需加入 version
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = "Already Save Data!";
                    return result;
                }
                foreach (var item in dataModel)
                {
                    now_id = Convert.ToInt32(item.Reference_Nbr);
                    db.Bond_Account_Info.Add(
                    new Bond_Account_Info()
                    {
                        Reference_Nbr = now_id,
                        Bond_Number = item.Bond_Number,
                        Lots = item.Lots,
                        Segment_Name = item.Segment_Name,
                        Curr_Sp_Issuer = item.Curr_Sp_Issuer,
                        Curr_Moodys_Issuer = item.Curr_Moodys_Issuer,
                        Curr_Fitch_Issuer = item.Curr_Fitch_Issuer,
                        Curr_Tw_Issuer = item.Curr_Tw_Issuer,
                        Curr_Sp_Issue = item.Curr_Sp_Issue,
                        Curr_Moodys_Issue = item.Curr_Moodys_Issue,
                        Curr_Fitch_Issue = item.Curr_Fitch_Issue,
                        Curr_Tw_Issue = item.Curr_Tw_Issue,
                        Ori_Amount = TypeTransfer.stringToDoubleN(item.Ori_Amount),
                        Current_Int_Rate = TypeTransfer.stringToDoubleN(item.Current_Int_Rate),
                        Origination_Date = TypeTransfer.stringToDateTimeN(item.Origination_Date),
                        Maturity_Date = TypeTransfer.stringToDateTimeN(item.Maturity_Date),
                        Principal_Payment_Method_Code = item.Principal_Payment_Method_Code,
                        Payment_Frequency = item.Payment_Frequency,
                        Balloon_Date = item.Balloon_Date,
                        Issuer_Area = item.Issuer_Area,
                        Industry_Sector = item.Industry_Sector,
                        Product = item.Product,
                        Finance_Instruments = item.Finance_Instruments,
                        Ias39_Category = item.Ias39_Category,
                        Principal = TypeTransfer.stringToDoubleN(item.Principal),
                        Amort_Amt_Tw = TypeTransfer.stringToDoubleN(item.Amort_Amt_Tw),
                        Interest_Receivable = TypeTransfer.stringToDoubleN(item.Interest_Receivable),
                        Interest_Receivable_tw = TypeTransfer.stringToDoubleN(item.Interest_Receivable_tw),
                        Interest_Rate_Type = item.Interest_Rate_Type,
                        Impair_Yn = item.Impair_Yn,
                        Eir = TypeTransfer.stringToDoubleN(item.Eir),
                        Currency_Code = item.Currency_Code,
                        Report_Date = TypeTransfer.stringToDateTimeN(item.Report_Date),
                        Issuer = item.Issuer,
                        Country_Risk = item.Country_Risk,
                        Ex_rate = TypeTransfer.stringToDoubleN(item.Ex_rate),
                        Lien_position = item.Lien_position,
                        Portfolio = item.Portfolio,
                        Dept = item.Dept,
                        Asset_Seg = item.Asset_Seg,
                        Ori_Ex_rate = TypeTransfer.stringToDoubleN(item.Ori_Ex_rate),
                        Bond_Type = item.Bond_Type,
                        Assessment_Sub_Kind = item.Assessment_Sub_Kind,
                        Processing_Date = TypeTransfer.stringToDateTimeN(item.Processing_Date),
                        Version = item.Version
                    });
                }
                db.SaveChanges(); //Save
                result.RETURN_FLAG = true;
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = ex.Message + 
                    " Reference_Nbr : "+ now_id.ToString();
            }
            return result;
        }
        #endregion

        #region Save B01
        public MSGReturnModel saveB01(string version ,DateTime date)
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Ref.Message_Type
                    .not_Find_Any.GetDescription();
                if (db.Bond_Account_Info.Count() > 0)
                {
                    List<Bond_Account_Info> addData = //這次要新增的資料
                        db.Bond_Account_Info.AsEnumerable()
                        .Where(x => x.Report_Date != null &&
                        date.Equals(x.Report_Date.Value) //抓取相同的Report_date
                        && version.Equals(x.Version)).ToList();  //抓取相同的 Verison
                    if (0.Equals(addData.Count))
                    {
                        result.DESCRIPTION = Ref.Message_Type
                            .query_Not_Find.GetDescription();
                        return result;
                    }

                    if (db.IFRS9_Main.Count() > 0)
                    {
                        List<int> B01Ids = new List<int>();
                        B01Ids.AddRange(db.IFRS9_Main.AsEnumerable()
                        .Select(x => x.Reference_Nbr).ToList()); //抓取 B01PK
                        addData = addData.Where(x => 
                        !B01Ids.Contains(x.Reference_Nbr)).ToList(); //排除 save 重複資料
                    }

                    if (0.Equals(addData.Count))
                    {
                        result.DESCRIPTION = Ref.Message_Type
                            .already_Save.GetDescription();
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
                            Current_Int_Rate = x.Current_Int_Rate, //
                            //Current_Pd = //
                            //Current_Lgd = //
                            //Remaining_Month = //
                            Eir = x.Eir, //
                            //CPD_Segment_Code = //
                            //Processing_Date = //
                            Product_Code = x.Principal_Payment_Method_Code, //
                            //Department = //
                            //PD_Model_Code = //
                            //18 //Current_Rating_Code = x.
                            Report_Date = x.Report_Date,
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
                            //Payment_Frequency = x.Payment_Frequency
                        };
                    }));
                    db.SaveChanges();
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Ref.Message_Type
                        .save_Success.GetDescription();
                }
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = ex.Message;
            }
            return result;
        }
        #endregion

        public MSGReturnModel saveC01(string version, DateTime date)
        {
            MSGReturnModel result = new MSGReturnModel();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.Message_Type
                    .not_Find_Any.GetDescription();
            try
            {
                if (db.IFRS9_Main.Count() > 0)
                {
                    List<IFRS9_Main> addData = //這次要新增的資料
                        db.IFRS9_Main.AsEnumerable()
                        .Where(x => x.Report_Date != null &&
                        date.Equals(x.Report_Date.Value) //抓取相同的Report_date
                        && version.Equals(x.Version)).ToList();  //抓取相同的 Verison

                    if (0.Equals(addData.Count))
                    {
                        result.DESCRIPTION = Ref.Message_Type
                            .query_Not_Find.GetDescription();
                        return result;
                    }

                    foreach (IFRS9_Main item in addData)
                    {
                        //if(db.EL_Data_In.SingleOrDefault(x=>x.))
                    }

                }
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = ex.Message;
            }
            return result;
        }

        #endregion

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
                if (db.Bond_Account_Info.Count() > 0)
                    idNum = db.Bond_Account_Info.AsEnumerable().Max(x => x.Reference_Nbr);
                if (resultData.Tables[0].Rows.Count > 2) //判斷有無資料
                {
                    dataModel = resultData.Tables[1].AsEnumerable().Skip(1) //第二頁籤第二行開始
                        .Select((x, y) =>
                        {
                            return getA41Model(x, (y + 1+ idNum).ToString());
                        }
                        ).ToList();

                    //skip(1) 為排除第一行 Excel Title列那行(參數可調)

                }
            }
            catch (Exception ex)
            { }
            return dataModel;
        }
        #endregion

        #endregion

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
                Maturity_Date = TypeTransfer.objDateToString(item[14]), //O (缺 P=>15,Q=>16) 到期日
                Principal_Payment_Method_Code = Principal_Payment_Method_Code,
                Payment_Frequency = string.Empty,
                Balloon_Date = TypeTransfer.objToString(item[17]), //R (缺 S=>18) 贖回日期(本金一次贖回)
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
        #endregion

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
                Reference_Nbr = data.Reference_Nbr.ToString(),
                Bond_Number = data.Bond_Number,
                Lots = data.Lots,
                Segment_Name = data.Segment_Name,
                Curr_Sp_Issuer = data.Curr_Sp_Issuer,
                Curr_Moodys_Issuer = data.Curr_Moodys_Issuer,
                Curr_Fitch_Issuer = data.Curr_Fitch_Issuer,
                Curr_Tw_Issuer = data.Curr_Tw_Issuer,
                Curr_Sp_Issue = data.Curr_Sp_Issue,
                Curr_Moodys_Issue = data.Curr_Moodys_Issue,
                Curr_Fitch_Issue = data.Curr_Fitch_Issue,
                Curr_Tw_Issue = data.Curr_Tw_Issue,
                Ori_Amount = TypeTransfer.doubleNToString(data.Ori_Amount),
                Current_Int_Rate = TypeTransfer.doubleNToString(data.Current_Int_Rate),
                Origination_Date = TypeTransfer.dateTimeNToString(data.Origination_Date),
                Maturity_Date = TypeTransfer.dateTimeNToString(data.Maturity_Date),
                Principal_Payment_Method_Code = data.Principal_Payment_Method_Code,
                Payment_Frequency = data.Payment_Frequency,
                Balloon_Date = data.Balloon_Date,
                Issuer_Area = data.Issuer_Area,
                Industry_Sector = data.Industry_Sector,
                Product = data.Product,
                Finance_Instruments = data.Finance_Instruments,
                Ias39_Category = data.Ias39_Category,
                Principal = TypeTransfer.doubleNToString(data.Principal),
                Amort_Amt_Tw = TypeTransfer.doubleNToString(data.Amort_Amt_Tw),
                Interest_Receivable = TypeTransfer.doubleNToString(data.Interest_Receivable),
                Interest_Receivable_tw = TypeTransfer.doubleNToString(data.Interest_Receivable_tw),
                Interest_Rate_Type = data.Interest_Rate_Type,
                Impair_Yn = data.Impair_Yn,
                Eir = TypeTransfer.doubleNToString(data.Eir),
                Currency_Code = data.Currency_Code,
                Report_Date = TypeTransfer.dateTimeNToString(data.Report_Date),
                Issuer = data.Issuer,
                Country_Risk = data.Country_Risk,
                Ex_rate = TypeTransfer.doubleNToString(data.Ex_rate),
                Lien_position = data.Lien_position,
                Portfolio = data.Portfolio,
                Dept = data.Dept,
                Asset_Seg = data.Asset_Seg,
                Ori_Ex_rate = TypeTransfer.doubleNToString(data.Ori_Ex_rate),
                Bond_Type = data.Bond_Type,
                Assessment_Sub_Kind = data.Assessment_Sub_Kind,
                Processing_Date = TypeTransfer.dateTimeNToString(data.Processing_Date),
                Version = data.Version
            };
        }
        #endregion

        #endregion
    }
}