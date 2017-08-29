﻿using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Transfer.Models.Interface;
using Transfer.Utility;
using Transfer.ViewModels;
using static Transfer.Enum.Ref;

namespace Transfer.Models.Repository
{
    public class A5Repository : IA5Repository, IDbEvent
    {
        #region 其他

        public A5Repository()
        {
            this.db = new IFRS9Entities();
        }

        protected IFRS9Entities db
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

        public Tuple<bool, List<A58ViewModel>> GetA58(
            string datepicker,
            string sType,
            string from,
            string to,
            string bondNumber,
            string version,
            string search)
        {
            DateTime? dp = TypeTransfer.stringToDateTimeN(datepicker);
            DateTime? df = TypeTransfer.stringToDateTimeN(from);
            DateTime? dt = TypeTransfer.stringToDateTimeN(to);
            int ver = 0;
            Int32.TryParse(version.Trim(), out ver);
            var data = db.Bond_Rating_Summary.AsQueryable();
            if (dp.HasValue)
                data = data.Where(x => x.Report_Date != null &&
                                       x.Report_Date == dp.Value &&
                                       x.Version != 0 &&
                                       x.Version == ver);
            if (df.HasValue && dt.HasValue)
                data = data.Where(x => x.Origination_Date != null &&
                                       x.Origination_Date >= df &&
                                       x.Origination_Date <= dt);
            if (!sType.IsNullOrWhiteSpace())
                data = data.Where(x => x.Rating_Type == sType);
            if (!bondNumber.IsNullOrWhiteSpace())
                data = data.Where(x => x.Bond_Number == bondNumber.Trim());
            if ("Miss".Equals(search))
                data = data.Where(x => x.Grade_Adjust == null);
            if (data.Any())
            {
                return new Tuple<bool, List<A58ViewModel>>(true,
                    data.AsEnumerable().Select(x => { return getA58ViewModel(x); }).ToList());
            }
            else
                return new Tuple<bool, List<A58ViewModel>>(false, new List<A58ViewModel>());
        }

        #endregion Get Data

        #region Excel 部分

        #region 下載 Excel

        /// <summary>
        /// 下載 Excel
        /// </summary>
        /// <param name="type">(A59)</param>
        /// <param name="path">下載位置</param>
        /// <param name="cache">cache 資料</param>
        /// <returns></returns>
        public MSGReturnModel DownLoadExcel<T>(string type, string path, List<T> data)
        {
            MSGReturnModel result = new MSGReturnModel();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Message_Type.download_Fail
                .GetDescription(type, Message_Type.not_Find_Any.GetDescription());
            if (Excel_DownloadName.A59.ToString().Equals(type))
            {
                List<A58ViewModel> A58Data = data.Cast<A58ViewModel>().ToList();
                DataTable dt = A58Data.Select(x => getA59ViewModel(x)).ToList().ToDataTable();
                result.DESCRIPTION = FileRelated.DataTableToExcel(dt, path, Excel_DownloadName.A59);
                result.RETURN_FLAG = string.IsNullOrWhiteSpace(result.DESCRIPTION);
            }
            return result;
        }

        #endregion 下載 Excel

        #region Excel 資料轉成 A59ViewModel

        /// <summary>
        /// Excel 資料轉成 A59ViewModel
        /// </summary>
        /// <param name="pathType">Excel 副檔名</param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public List<A59ViewModel> getA59Excel(string pathType, Stream stream)
        {
            DataSet resultData = new DataSet();
            List<A59ViewModel> dataModel = new List<A59ViewModel>();
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
                if (resultData.Tables[0].Rows.Count > 0) //判斷有無資料
                {
                    dataModel = resultData.Tables[0].AsEnumerable().Skip(0) 
                        .Select((x, y) =>
                        {
                            return getA59ViewModelInExcel(x);
                        }
                        ).ToList();
                }
            }
            catch (Exception ex)
            { }
            return dataModel;
        }

        #endregion Excel 資料轉成 A59ViewModel

        #endregion Excel 部分

        #region Private Function

        #region Bond_Rating_Summary 組成 A58ViewModel

        /// <summary>
        /// Bond_Rating_Summary 組成 A58ViewModel
        /// </summary>
        /// <param name="item">DataRow</param>
        /// <returns>A58ViewModel</returns>
        private A58ViewModel getA58ViewModel(Bond_Rating_Summary item)
        {
            return new A58ViewModel()
            {
                Reference_Nbr = item.Reference_Nbr,
                Report_Date = TypeTransfer.dateTimeNToString(item.Report_Date),
                Bond_Number = item.Bond_Number,
                Lots = item.Lots,
                Origination_Date = TypeTransfer.dateTimeNToString(item.Origination_Date),
                Parm_ID = item.Parm_ID,
                Bond_Type = item.Bond_Type,
                Rating_Type = item.Rating_Type == "1" ? Rating_Type.A.GetDescription() : Rating_Type.B.GetDescription(),
                Rating_Object = item.Rating_Object,
                Rating_Org_Area = item.Rating_Org_Area,
                Rating_Selection = item.Rating_Selection == "1" ? "孰高" : "孰低",
                Grade_Adjust = TypeTransfer.intNToString(item.Grade_Adjust),
                Rating_Priority = TypeTransfer.intNToString(item.Rating_Priority),
                Processing_Date = TypeTransfer.dateTimeNToString(item.Processing_Date),
                Version = TypeTransfer.intNToString(item.Version),
                Portfolio_Name = item.Portfolio_Name,
                SMF = item.SMF,
                Issuer = item.ISSUER,
                Security_Ticker = getSecurityTicker(item.SMF, item.Bond_Number),
                RATING_AS_OF_DATE_OVERRIDE = item.Rating_Type == "1" ?
                TypeTransfer.dateTimeNToString(item.Origination_Date, 8) :
                TypeTransfer.dateTimeNToString(item.Report_Date, 8)
            };
        }

        #endregion Bond_Rating_Summary 組成 A58ViewModel

        #region A58ViewModel 組成 A59ViewModel

        private A59ViewModel getA59ViewModel(A58ViewModel item)
        {
            return new A59ViewModel()
            {
                Reference_Nbr = item.Reference_Nbr,
                Report_Date = item.Report_Date,
                Version = item.Version,
                Bond_Number = item.Bond_Number,
                Lots = item.Lots,
                Origination_Date = item.Origination_Date,
                Portfolio_Name = item.Portfolio_Name,
                SMF = item.SMF,
                Issuer = item.Issuer,
                Security_Ticker = item.Security_Ticker,
                RATING_AS_OF_DATE_OVERRIDE = item.RATING_AS_OF_DATE_OVERRIDE
            };
        }

        #endregion A58ViewModel 組成 A59ViewModel

        #region Excel 組成 A59ViewModel

        private A59ViewModel getA59ViewModelInExcel(DataRow item)
        {
            return new A59ViewModel()
            {
                Reference_Nbr = TypeTransfer.objToString(item[0]), //帳戶編號
                Report_Date = TypeTransfer.objToString(item[1]), //報導日
                Version = TypeTransfer.objToString(item[2]), //資料版本
                Bond_Number = TypeTransfer.objToString(item[3]), //債券編號
                Lots = TypeTransfer.objToString(item[4]), //Lots
                Origination_Date = TypeTransfer.objToString(item[5]), //債券購入(認列)日期
                Portfolio_Name = TypeTransfer.objToString(item[6]), //Portfolio英文
                SMF = TypeTransfer.objToString(item[7]), //債券產品別(揭露使用)
                Issuer = TypeTransfer.objToString(item[8]), //Issuer
                Security_Ticker = TypeTransfer.objToString(item[9]), //Security_Ticker
                RATING_AS_OF_DATE_OVERRIDE = TypeTransfer.objToString(item[10]), //RATING_AS_OF_DATE_OVERRIDE
                ISSUER_TICKER = TypeTransfer.objToString(item[11]), //ISSUER_TICKER
                GUARANTOR_NAME = TypeTransfer.objToString(item[12]), //GUARANTOR_NAME
                GUARANTOR_EQY_TICKER = TypeTransfer.objToString(item[13]), //GUARANTOR_EQY_TICKER
                RTG_SP = TypeTransfer.objToString(item[14]), //債項_標普評等 (債項\ sp\國外)
                SP_EFF_DT = TypeTransfer.objToString(item[15]), //債項_標普評等日期
                RTG_TRC = TypeTransfer.objToString(item[16]), //債項_TRC 評等 (債項\ CW\國內)
                TRC_EFF_DT = TypeTransfer.objToString(item[17]), //債項_TRC 評等日期
                RTG_MOODY = TypeTransfer.objToString(item[18]), //債項_穆迪評等 (債項\ moody\國外)
                MOODY_EFF_DT = TypeTransfer.objToString(item[19]), //債項_穆迪評等日期
                RTG_FITCH = TypeTransfer.objToString(item[20]), //債項_惠譽評等 (債項\ Fitch\國外)
                FITCH_EFF_DT = TypeTransfer.objToString(item[21]), //債項_惠譽評等日期
                RTG_FITCH_NATIONAL = TypeTransfer.objToString(item[22]), //債項_惠譽國內評等 (債項\ Fitch(twn)\國內)
                RTG_FITCH_NATIONAL_DT = TypeTransfer.objToString(item[23]), //債項_惠譽國內評等日期
                RTG_SP_LT_FC_ISSUER_CREDIT = TypeTransfer.objToString(item[24]), //標普長期外幣發行人信用評等 (發行人\ sp\國外)
                RTG_SP_LT_FC_ISS_CRED_RTG_DT = TypeTransfer.objToString(item[25]), //標普長期外幣發行人信用評等日期
                RTG_SP_LT_LC_ISSUER_CREDIT = TypeTransfer.objToString(item[26]),//標普本國貨幣長期發行人信用評等 (發行人\ sp\國外)
                RTG_SP_LT_LC_ISS_CRED_RTG_DT = TypeTransfer.objToString(item[27]), //標普本國貨幣長期發行人信用評等日期
                RTG_MDY_ISSUER = TypeTransfer.objToString(item[28]), //穆迪發行人評等 (發行人\ moody\國外)
                RTG_MDY_ISSUER_RTG_DT = TypeTransfer.objToString(item[29]), //穆迪發行人評等日期
                RTG_MOODY_LONG_TERM = TypeTransfer.objToString(item[30]), //發行人_穆迪長期評等 (發行人\ moody\國外)
                RTG_MOODY_LONG_TERM_DATE = TypeTransfer.objToString(item[31]), //發行人_穆迪長期評等日期
                RTG_MDY_SEN_UNSECURED_DEBT = TypeTransfer.objToString(item[32]), //發行人_穆迪優先無擔保債務評等 (發行人\ moody\國外)
                RTG_MDY_SEN_UNSEC_RTG_DT = TypeTransfer.objToString(item[33]), //發行人_穆迪優先無擔保債務評等_日期
                RTG_MDY_FC_CURR_ISSUER_RATING = TypeTransfer.objToString(item[34]), //穆迪外幣發行人評等 (發行人\ moody\國外)
                RTG_MDY_FC_CURR_ISSUER_RTG_DT = TypeTransfer.objToString(item[35]), //穆迪外幣發行人評等日期
                RTG_MDY_LOCAL_LT_BANK_DEPOSITS = TypeTransfer.objToString(item[36]), //發行人_穆迪長期本國銀行存款評等 (發行人\ moody\國內)
                RTG_MDY_LT_LC_BANK_DEP_RTG_DT = TypeTransfer.objToString(item[37]), //發行人_穆迪長期本國銀行存款評等日期
                RTG_FITCH_LT_ISSUER_DEFAULT = TypeTransfer.objToString(item[38]), //惠譽長期發行人違約評等 (發行人\ Fitch\國外)
                RTG_FITCH_LT_ISSUER_DFLT_RTG_DT = TypeTransfer.objToString(item[39]), //惠譽長期發行人違約評等日期
                RTG_FITCH_SEN_UNSECURED = TypeTransfer.objToString(item[40]), //發行人_惠譽優先無擔保債務評等 (發行人\ Fitch\國外)
                RTG_FITCH_SEN_UNSEC_RTG_DT = TypeTransfer.objToString(item[41]), //發行人_惠譽優先無擔保債務評等日期
                RTG_FITCH_LT_FC_ISSUER_DEFAULT = TypeTransfer.objToString(item[42]), //惠譽長期外幣發行人違約評等 (發行人\ Fitch\國外)
                RTG_FITCH_LT_FC_ISS_DFLT_RTG_DT = TypeTransfer.objToString(item[43]),  //惠譽長期外幣發行人違約評等日期
                RTG_FITCH_LT_LC_ISSUER_DEFAULT = TypeTransfer.objToString(item[44]), //惠譽長期本國貨幣發行人違約評等 (發行人\ Fitch\國外)
                RTG_FITCH_LT_LC_ISS_DFLT_RTG_DT = TypeTransfer.objToString(item[45]), //惠譽長期本國貨幣發行人違約評等日期
                RTG_FITCH_NATIONAL_LT = TypeTransfer.objToString(item[46]), //發行人_惠譽國內長期評等 (發行人\ Fitch(twn)\國內)
                RTG_FITCH_NATIONAL_LT_DT = TypeTransfer.objToString(item[47]), //發行人_惠譽國內長期評等日期
                RTG_TRC_LONG_TERM = TypeTransfer.objToString(item[48]), //發行人_TRC 長期評等 (發行人\ CW\國內)
                RTG_TRC_LONG_TERM_RTG_DT = TypeTransfer.objToString(item[49]), //發行人_TRC 長期評等日期
                G_RTG_SP_LT_FC_ISSUER_CREDIT = TypeTransfer.objToString(item[50]), //標普長期外幣保證人信用評等 (保證人\ sp\國外)
                G_RTG_SP_LT_FC_ISS_CRED_RTG_DT = TypeTransfer.objToString(item[51]), //標普長期外幣保證人信用評等日期
                G_RTG_SP_LT_LC_ISSUER_CREDIT = TypeTransfer.objToString(item[52]), //標普本國貨幣長期保證人信用評等 (保證人\ sp\國外)
                G_RTG_SP_LT_LC_ISS_CRED_RTG_DT = TypeTransfer.objToString(item[53]), //標普本國貨幣長期保證人信用評等日期
                G_RTG_MDY_ISSUER = TypeTransfer.objToString(item[54]), //穆迪保證人評等 (保證人\ moody\國外)
                G_RTG_MDY_ISSUER_RTG_DT = TypeTransfer.objToString(item[55]), //穆迪保證人評等日期
                G_RTG_MOODY_LONG_TERM = TypeTransfer.objToString(item[56]), //保證人_穆迪長期評等 (保證人\ moody\國外)
                G_RTG_MOODY_LONG_TERM_DATE = TypeTransfer.objToString(item[57]), //保證人_穆迪長期評等日期
                G_RTG_MDY_SEN_UNSECURED_DEBT = TypeTransfer.objToString(item[58]), //保證人_穆迪優先無擔保債務評等 (保證人\ moody\國外)
                G_RTG_MDY_SEN_UNSEC_RTG_DT = TypeTransfer.objToString(item[59]), //保證人_穆迪優先無擔保債務評等_日期
                G_RTG_MDY_FC_CURR_ISSUER_RATING = TypeTransfer.objToString(item[60]), //穆迪外幣保證人評等 (保證人\ moody\國外)
                G_RTG_MDY_FC_CURR_ISSUER_RTG_DT = TypeTransfer.objToString(item[61]), //穆迪外幣保證人評等日期
                G_RTG_MDY_LOCAL_LT_BANK_DEPOSITS = TypeTransfer.objToString(item[62]), //保證人_穆迪長期本國銀行存款評等 (保證人\ moody\國內)
                G_RTG_MDY_LT_LC_BANK_DEP_RTG_DT = TypeTransfer.objToString(item[63]), //保證人_穆迪長期本國銀行存款評等日期
                G_RTG_FITCH_LT_ISSUER_DEFAULT = TypeTransfer.objToString(item[64]), //惠譽長期保證人違約評等 (保證人\ Fitch\國外)
                G_RTG_FITCH_LT_ISSUER_DFLT_RTG_DT = TypeTransfer.objToString(item[65]), //惠譽長期保證人違約評等日期
                G_RTG_FITCH_SEN_UNSECURED = TypeTransfer.objToString(item[66]), //保證人_惠譽優先無擔保債務評等 (保證人\ Fitch\國外)
                G_RTG_FITCH_SEN_UNSEC_RTG_DT = TypeTransfer.objToString(item[67]), //保證人_惠譽優先無擔保債務評等日期
                G_RTG_FITCH_LT_FC_ISSUER_DEFAULT = TypeTransfer.objToString(item[68]), //惠譽長期外幣保證人違約評等 (保證人\ Fitch\國外)
                G_RTG_FITCH_LT_FC_ISS_DFLT_RTG_DT = TypeTransfer.objToString(item[69]), //惠譽長期外幣保證人違約評等日期
                G_RTG_FITCH_LT_LC_ISSUER_DEFAULT = TypeTransfer.objToString(item[70]), //惠譽長期本國貨幣保證人違約評等 (保證人\ Fitch\國外)
                G_RTG_FITCH_LT_LC_ISS_DFLT_RTG_DT = TypeTransfer.objToString(item[71]), //惠譽長期本國貨幣保證人違約評等日期
                G_RTG_FITCH_NATIONAL_LT = TypeTransfer.objToString(item[72]), //保證人_惠譽國內長期評等 (保證人\ Fitch(twn)\國內)
                G_RTG_FITCH_NATIONAL_LT_DT = TypeTransfer.objToString(item[73]), //保證人_惠譽國內長期評等日期
                G_RTG_TRC_LONG_TERM = TypeTransfer.objToString(item[74]), //保證人_TRC 長期評等 (保證人\ CW\國內)
                G_RTG_TRC_LONG_TERM_RTG_DT = TypeTransfer.objToString(item[75]), //保證人_TRC 長期評等日期
                //Processing_Date =
            };
        }

        #endregion Excel 組成 A59ViewModel

        /// <summary>
        /// get Security_Ticker
        /// </summary>
        /// <param name="SMF"></param>
        /// <param name="bondNumber"></param>
        /// <returns></returns>
        private string getSecurityTicker(string SMF, string bondNumber)
        {
            List<string> Mtges = new List<string>() { "A11", "932" };
            if (!SMF.IsNullOrWhiteSpace() && SMF.Trim().Length > 3)
                if (Mtges.Contains(SMF.Substring(0, 3)))
                    return string.Format("{0} {1}", bondNumber, "Mtge");
            return string.Format("{0} {1}", bondNumber, "Corp");
        }

        #endregion Private Function
    }
}