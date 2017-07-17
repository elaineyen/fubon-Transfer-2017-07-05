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
                    dataModel = (from q in resultData.Tables[0].AsEnumerable()
                                 select getA41Model(q)).Skip(1).ToList();
                    //skip(1) 為排除Excel Title列那行(參數可調)
                    //dataModel = dataModel.Take(dataModel.Count - 1).ToList();
                    ////排除最後一筆 為 * Data in percent 的註解 
                }
            }
            catch
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
        private A41ViewModel getA41Model(DataRow item)
        {
            return new A41ViewModel()
            {
                Reference_Nbr = TypeTransfer.objToString(item[0]),
                Bond_Number = TypeTransfer.objToString(item[1]),
                Lots = TypeTransfer.objToString(item[2]),
                Segment_Name = TypeTransfer.objToString(item[3]),
                Curr_Sp_Issuer = TypeTransfer.objToString(item[4]),
                Curr_Moodys_Issuer = TypeTransfer.objToString(item[5]),
                Curr_Fitch_Issuer = TypeTransfer.objToString(item[6]),
                Curr_Tw_Issuer = TypeTransfer.objToString(item[7]),
                Curr_Sp_Issue = TypeTransfer.objToString(item[8]),
                Curr_Moodys_Issue = TypeTransfer.objToString(item[9]),
                Curr_Fitch_Issue = TypeTransfer.objToString(item[10]),
                Curr_Tw_Issue = TypeTransfer.objToString(item[11]),
                Ori_Amount = TypeTransfer.objToString(item[12]),
                Current_Int_Rate = TypeTransfer.objToString(item[13]),
                Origination_Date = TypeTransfer.objToString(item[14]),
                Maturity_Date = TypeTransfer.objToString(item[15]),
                Principal_Payment_Method_Code = TypeTransfer.objToString(item[16]),
                Payment_Frequency = TypeTransfer.objToString(item[17]),
                Balloon_Date = TypeTransfer.objToString(item[18]),
                Issuer_Area = TypeTransfer.objToString(item[19]),
                Industry_Sector = TypeTransfer.objToString(item[20]),
                Product = TypeTransfer.objToString(item[21]),
                Finance_Instruments = TypeTransfer.objToString(item[22]),
                Ias39_Category = TypeTransfer.objToString(item[23]),
                Principal = TypeTransfer.objToString(item[24]),
                Amort_Amt_Tw = TypeTransfer.objToString(item[25]),
                Interest_Receivable = TypeTransfer.objToString(item[26]),
                Interest_Receivable_tw = TypeTransfer.objToString(item[27]),
                Interest_Rate_Type = TypeTransfer.objToString(item[28]),
                Impair_Yn = TypeTransfer.objToString(item[29]),
                Eir = TypeTransfer.objToString(item[30]),
                Currency_Code = TypeTransfer.objToString(item[31]),
                Report_Date = TypeTransfer.objToString(item[32]),
                Issuer = TypeTransfer.objToString(item[33]),
                Country_Risk = TypeTransfer.objToString(item[34]),
                Ex_rate = TypeTransfer.objToString(item[35]),
                Lien_position = TypeTransfer.objToString(item[36]),
                Portfolio = TypeTransfer.objToString(item[37]),
                Dept = TypeTransfer.objToString(item[38]),
                Asset_Seg = TypeTransfer.objToString(item[39]),
                Ori_Ex_rate = TypeTransfer.objToString(item[40]),
                Bond_Type = TypeTransfer.objToString(item[41]),
                Assessment_Sub_Kind = TypeTransfer.objToString(item[42]),
                Processing_Date = TypeTransfer.objToString(item[43]),
                Version = TypeTransfer.objToString(item[44])
            };
        }
        #endregion
    }
}