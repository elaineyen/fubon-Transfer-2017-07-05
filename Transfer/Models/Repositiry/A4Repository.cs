using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using Transfer.Models.Interface;
using Transfer.Utility;
using Transfer.ViewModels;

namespace Transfer.Models.Repositiry
{
    public class A4Repository : IA4Repository , IDbEvent
    {
        protected IFRS9Entities db
        {
            get;
            private set;
        }

        public A4Repository()
        {
            this.db = new IFRS9Entities();
        }

        public bool CreateA41(List<Exhibit29Model> model)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Grade_Moody_Info> GetAll()
        {
            throw new NotImplementedException();
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

        #region Excel 部分

        #region Excel 資料轉成 Exhibit29Model
        /// <summary>
        /// Excel 資料轉成 Exhibit29Model
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

                if (resultData.Tables[0].Rows.Count > 2) //判斷有無資料
                {
                    dataModel = resultData.Tables[1].AsEnumerable().Skip(1)
                        .Select((x, y) => {
                            return getA41Model(x, (y + 1).ToString().PadLeft(10, '0')); }
                        ).ToList();

                        //(from q in resultData.Tables[1].AsEnumerable()
                                 //第二頁籤(All) 第一頁籤(欄位定義說明)
                                 //select getA41Model(q)).Skip(2).ToList();
                    //skip(1) 為排除Excel Title列那行(參數可調)
                    //dataModel = dataModel.Take(dataModel.Count - 1).ToList();
                    ////排除最後一筆 為 * Data in percent 的註解 
                }
            }
            catch (Exception ex)
            { }
            return dataModel;
        }
        #endregion

        #endregion

        #region datarow 組成 A41ViewModel
        /// <summary>
        /// datarow 組成 A41ViewModel
        /// </summary>
        /// <param name="item">DataRow</param>
        /// <returns>A41ViewModel</returns>
        private A41ViewModel getA41Model(DataRow item,string num)
        {
            var Maturity_Date = TypeTransfer.objToString(item[14]);
            DateTime MDate = DateTime.MinValue;          
            var Balloon_Date = TypeTransfer.objToString(item[16]);
            string Principal_Payment_Method_Code = "01";
            // Balloon_Date IN ('0') OR  Balloon_Date IS NULL THEN '02'
            if (Maturity_Date.IsNullOrWhiteSpace() || "0".Equals(Maturity_Date))
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
                Origination_Date = TypeTransfer.objToString(item[13]), //N 債券購入(認列)日期
                Maturity_Date = TypeTransfer.objToString(item[14]), //O (缺 P=>15,Q=>16) 到期日
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
                Report_Date = TypeTransfer.objToString(item[33]), //AH 評估基準日/報導日
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
    }
}