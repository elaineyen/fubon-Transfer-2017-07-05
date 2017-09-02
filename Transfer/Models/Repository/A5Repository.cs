using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Reflection;
using Transfer.Models.Interface;
using Transfer.Utility;
using Transfer.ViewModels;
using static AutoTransfer.Enum.Ref;
using static Transfer.Enum.Ref;

namespace Transfer.Models.Repository
{
    public class A5Repository : IA5Repository, IDbEvent
    {
        #region 其他

        public A5Repository()
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
        /// get A58 Data
        /// </summary>
        /// <param name="datepicker">ReportDate</param>
        /// <param name="sType">Rating_Type</param>
        /// <param name="from">Origination_Date start</param>
        /// <param name="to">Origination_Date to</param>
        /// <param name="bondNumber">bondNumber</param>
        /// <param name="version">version</param>
        /// <param name="search">全部or缺漏</param>
        /// <returns></returns>
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

        /// <summary>
        /// get 轉檔紀錄Table 資料
        /// </summary>
        /// <returns></returns>
        public List<CheckTableViewModel> getCheckTable()
        {
            List<CheckTableViewModel> data = new List<CheckTableViewModel>();
            if (db.Transfer_CheckTable.Any(x => x.File_Name == Table_Type.A41.ToString()))
                data = db.Transfer_CheckTable.Where(x =>
                x.File_Name == Table_Type.A41.ToString())
                .OrderBy(x => x.ReportDate).ThenBy(x => x.Version)
                .ThenByDescending(x => x.TransferType)
                .AsEnumerable()
                .Select(x => new CheckTableViewModel()
                {
                    ReportDate = x.ReportDate.ToString("yyyy/MM/dd"),
                    Version = x.Version.ToString(),
                    TransferType = x.TransferType,
                    Create_Date = x.Create_date,
                    Create_Time = x.Create_time,
                    End_Date = x.End_date,
                    End_Time = x.End_time
                }).ToList();
            return data;
        }

        #endregion Get Data

        #region Save Db

        /// <summary>
        /// save A59 (save A5 & A58)
        /// </summary>
        /// <param name="dataModel">A59ViewModel</param>
        /// <returns></returns>
        public MSGReturnModel saveA59(List<A59ViewModel> dataModel)
        {
            MSGReturnModel result = new MSGReturnModel();
            if (!dataModel.Any())
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type.parameter_Error
                    .GetDescription(Table_Type.A59.ToString());
                return result;
            }

            DateTime startTime = DateTime.Now;
            string startDt = startTime.ToString("yyyy/MM/dd");
            string sql = string.Empty;


            
            dataModel.ForEach(x =>
            {
                var pros = x.GetType().GetProperties()
                            .Skip(15).TakeWhile(z => z.Name != "Processing_Date").ToList();
                var rateInfo = pros.Where(i => !i.GetValue(x).ToString().IsNullOrWhiteSpace() &&
                                               !i.Name.EndsWith("DT")).FirstOrDefault();

                var dtInfo = pros.Where(i => !i.GetValue(x).ToString().IsNullOrWhiteSpace() &&
                                              i.Name.EndsWith("DT")).FirstOrDefault();
                string rate = null; //Rate
                if (rateInfo != null)
                    rate = forRating(rateInfo.GetValue(x).ToString());
                DateTime? rateDate = null; //RateDate
                if (dtInfo != null)
                    rateDate = TypeTransfer.stringToDateTimeN(dtInfo.GetValue(x).ToString(), 8);
                string rateDt = rateDate.HasValue ? rateDate.Value.ToString("yyyy/MM/dd") : null;

                if (!rate.IsNullOrWhiteSpace())
                {
                    var ratingType = x.Rating_Type.Equals(Rating_Type.A.GetDescription()) ? "1" : "2";

                    #region Save A57
                    sql += $@"
Update Bond_Rating_Info 
Set       Rating =  '{rate}',
          Rating_Date = '{rateDt}',
	      Grade_Adjust = GMooInfo.Grade_Adjust,
	      Fill_up_Date = '{startDt}',
	      Fill_up_YN = 'Y'
From      Bond_Rating_Info BR_Info --A57
Left Join Grade_Mapping_Info GMapInfo --A52
on        BR_Info.Rating_Org = GMapInfo.Rating_Org
AND       GMapInfo.Rating = '{rate}'
Left Join Grade_Moody_Info GMooInfo --A51
on        GMapInfo.PD_Grade = GMooInfo.PD_Grade 
Where     BR_Info.Rating_Type = '{ratingType}'
AND       BR_Info.Bond_Number = '{x.Bond_Number}'
AND       BR_Info.Lots = '{x.Lots}'
AND       BR_Info.Portfolio_Name = '{x.Portfolio_Name}' ";
                    if (ratingType == "1")
                        sql += $" AND  BR_Info.Report_Date = '{x.Report_Date}' ; ";
                    if (ratingType == "2")
                        sql += $" AND  BR_Info.Origination_Date = '{x.Origination_Date}' ; ";
                    #endregion

                    #region Save A58
                    sql +=
$@" WITH TEMP AS (
   Select TOP 1 Grade_Adjust 
    From Bond_Rating_Info BR_Info
Where BR_Info.Rating_Type = '{ratingType}'
AND   BR_Info.Bond_Number = '{x.Bond_Number}'
AND   BR_Info.Lots = '{x.Lots}'
AND   BR_Info.Portfolio_Name = '{x.Portfolio_Name}' ";
                    if (ratingType == "1")
                        sql += $" AND  BR_Info.Report_Date = '{x.Report_Date}'  ";
                    if (ratingType == "2")
                        sql += $" AND  BR_Info.Origination_Date = '{x.Origination_Date}'  ";
                    sql += $@" )
Update Bond_Rating_Summary
Set Grade_Adjust = TEMP.Grade_Adjust,
    Processing_Date = CASE WHEN TEMP.Grade_Adjust is null 
	                  THEN null else '{startDt}' END
from Bond_Rating_Summary BR_Summary,TEMP
Where BR_Summary.Rating_Type = '{ratingType}'
AND  BR_Summary.Bond_Number = '{x.Bond_Number}'
AND  BR_Summary.Lots = '{x.Lots}'
AND  BR_Summary.Portfolio_Name = '{x.Portfolio_Name}' ";
                    if (ratingType == "1")
                        sql += $" AND  BR_Summary.Report_Date = '{x.Report_Date}' ; ";
                    if (ratingType == "2")
                        sql += $" AND  BR_Summary.Origination_Date = '{x.Origination_Date}' ; ";
                    #endregion
                }
            });

            try
            {
                db.Database.ExecuteSqlCommand(sql);
                result.RETURN_FLAG = true;
                result.DESCRIPTION = Message_Type.save_Success.GetDescription();
            }
            catch (DbUpdateException ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type
                                     .save_Fail.GetDescription(Table_Type.A59.ToString(),
                                     $"message: {ex.Message}" +
                                     $", inner message {ex.InnerException?.InnerException?.Message}");
            }
            return result;
        }

        /// <summary>
        /// 手動轉換 A57 & A58
        /// </summary>
        /// <param name="date"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public MSGReturnModel saveA57A58(DateTime dt, int version)
        {
            MSGReturnModel result = new MSGReturnModel();
            DateTime startTime = DateTime.Now;
            result.RETURN_FLAG = false;

            if (!common.checkTransferCheck(Table_Type.A57.ToString(), Table_Type.A41.ToString(), dt, version) ||
                !common.checkTransferCheck(Table_Type.A58.ToString(), Table_Type.A53.ToString(), dt, version))
            {
                result.DESCRIPTION = Message_Type.transferError.GetDescription();
            }
            else
            {
                var A41 = db.Bond_Account_Info
                    .Where(x => x.Report_Date == dt && x.Version == version);
                var A53Data = db.Rating_Info.Where(x => x.Report_Date == dt);
                var sampleInfos = db.Rating_Info_SampleInfo
                    .Where(x => x.Report_Date == dt);
                if (A41.Any() && A53Data.Any() && sampleInfos.Any())
                {
                    string parmID = getParmID(); //選取離今日最近的D60
                    string reportData = dt.ToString("yyyy/MM/dd");
                    string ver = version.ToString();
                    #region sql
                    string sql = string.Empty;
                    sql = $@"
Begin Try

WITH T0 AS (
   Select BA_Info.Reference_Nbr AS Reference_Nbr ,
          BA_Info.Bond_Number AS Bond_Number,
		  BA_Info.Lots AS Lots,
		  BA_Info.Portfolio AS Portfolio,
		  BA_Info.Segment_Name AS Segment_Name,
		  BA_Info.Bond_Type AS Bond_Type,
		  BA_Info.Lien_position AS Lien_position,
		  BA_Info.Origination_Date AS Origination_Date,
          BA_Info.Report_Date AS Report_Date,
		  RA_Info.Rating_Date AS Rating_Date,
		  RA_Info.Rating_Object AS Rating_Object,
          RA_Info.Rating_Org AS Rating_Org,
		  RA_Info.Rating AS Rating,
		  (CASE WHEN RA_Info.Rating_Org in ('SP','cnSP','Fitch') THEN '國外'
	            WHEN RA_Info.Rating_Org in ('Fitch(twn)','CW') THEN '國內'
	      END) AS Rating_Org_Area,
		  GMapInfo.PD_Grade AS PD_Grade,
		  GMooInfo.Grade_Adjust AS Grade_Adjust,
		  CASE WHEN RISI.ISSUER_TICKER in ('N.S.', 'N.A.') THEN null Else RISI.ISSUER_TICKER END AS ISSUER_TICKER,
		  CASE WHEN RISI.GUARANTOR_NAME in ('N.S.', 'N.A.') THEN null Else RISI.GUARANTOR_NAME END AS GUARANTOR_NAME,
		  CASE WHEN RISI.GUARANTOR_EQY_TICKER in ('N.S.', 'N.A.') THEN null Else RISI.GUARANTOR_EQY_TICKER END AS GUARANTOR_EQY_TICKER,
		  BA_Info.Portfolio_Name AS Portfolio_Name,
		  RA_Info.RTG_Bloomberg_Field AS RTG_Bloomberg_Field,
		  BA_Info.PRODUCT AS SMF,
		  BA_Info.ISSUER AS ISSUER,
		  BA_Info.Version AS Version,
		  (CASE WHEN BA_Info.PRODUCT like 'A11%' OR BA_Info.PRODUCT like '932%'
		  THEN BA_Info.Bond_Number + ' Mtge' ELSE
		    BA_Info.Bond_Number + ' Corp' END) AS Security_Ticker
   from  Bond_Account_Info BA_Info --A41
   Join Rating_Info RA_Info --A53
   on BA_Info.Bond_Number = RA_Info.Bond_Number   
   Left Join Grade_Mapping_Info GMapInfo --A52
   on RA_Info.Rating_Org = GMapInfo.Rating_Org
   AND RA_Info.Rating = GMapInfo.Rating
   Left Join Grade_Moody_Info GMooInfo --A51
   on GMapInfo.PD_Grade = GMooInfo.PD_Grade
   Left Join Rating_Info_SampleInfo RISI --A53 Sample
   on BA_Info.Bond_Number = RISI.Bond_Number
   AND RA_Info.Rating_Object = '債項'
   Where BA_Info.Report_Date = '{reportData}'
   And   BA_Info.Version = {ver}
)
Insert into Bond_Rating_Info
           (Reference_Nbr,
           Bond_Number,
           Lots,
           Portfolio,
           Segment_Name,
           Bond_Type,
           Lien_position,
           Origination_Date,
           Report_Date,
           Rating_Date,
           Rating_Type,
           Rating_Object,
           Rating_Org,
           Rating,
           Rating_Org_Area,
           PD_Grade,
           Grade_Adjust,
           ISSUER_TICKER,
           GUARANTOR_NAME,
           GUARANTOR_EQY_TICKER,
           Parm_ID,
           Portfolio_Name,
           RTG_Bloomberg_Field,
           SMF,
           ISSUER,
           Version,
           Security_Ticker)
Select     Reference_Nbr,
		   Bond_Number,
		   Lots,
           Portfolio,
           Segment_Name,
           Bond_Type,
           Lien_position,
           Origination_Date,
           Report_Date,
           Rating_Date,
           '1',
           Rating_Object,
           Rating_Org,
           Rating,
           Rating_Org_Area,
           PD_Grade,
           Grade_Adjust,
           ISSUER_TICKER,
           GUARANTOR_NAME,
           GUARANTOR_EQY_TICKER,
           '{parmID}',
           Portfolio_Name,
           RTG_Bloomberg_Field,
           SMF,
           ISSUER,
           Version,
           Security_Ticker
		   From
		   T0;
WITH T1 AS (
   Select BA_Info.Reference_Nbr AS Reference_Nbr ,
          BA_Info.Bond_Number AS Bond_Number,
		  BA_Info.Lots AS Lots,
		  BA_Info.Portfolio AS Portfolio,
		  BA_Info.Segment_Name AS Segment_Name,
		  BA_Info.Bond_Type AS Bond_Type,
		  BA_Info.Lien_position AS Lien_position,
		  BA_Info.Origination_Date AS Origination_Date,
          BA_Info.Report_Date AS Report_Date,
		  RA_Info.Rating_Date AS Rating_Date,
		  RA_Info.Rating_Object AS Rating_Object,
          RA_Info.Rating_Org AS Rating_Org,
		  oldA57.Rating AS Rating,
		  (CASE WHEN RA_Info.Rating_Org in ('SP','cnSP','Fitch') THEN '國外'
	            WHEN RA_Info.Rating_Org in ('Fitch(twn)','CW') THEN '國內'
	      END) AS Rating_Org_Area,
		  GMapInfo.PD_Grade AS PD_Grade,
		  GMooInfo.Grade_Adjust AS Grade_Adjust,
		  CASE WHEN RISI.ISSUER_TICKER in ('N.S.', 'N.A.') THEN null Else RISI.ISSUER_TICKER END AS ISSUER_TICKER,
		  CASE WHEN RISI.GUARANTOR_NAME in ('N.S.', 'N.A.') THEN null Else RISI.GUARANTOR_NAME END AS GUARANTOR_NAME,
		  CASE WHEN RISI.GUARANTOR_EQY_TICKER in ('N.S.', 'N.A.') THEN null Else RISI.GUARANTOR_EQY_TICKER END AS GUARANTOR_EQY_TICKER,
		  BA_Info.Portfolio_Name AS Portfolio_Name,
		  RA_Info.RTG_Bloomberg_Field AS RTG_Bloomberg_Field,
		  BA_Info.PRODUCT AS SMF,
		  BA_Info.ISSUER AS ISSUER,
		  BA_Info.Version AS Version,
		  (CASE WHEN BA_Info.PRODUCT like 'A11%' OR BA_Info.PRODUCT like '932%' 
		  THEN BA_Info.Bond_Number + ' Mtge' ELSE
		    BA_Info.Bond_Number + ' Corp' END) AS Security_Ticker
   from  Bond_Account_Info BA_Info --A41
   Join Rating_Info RA_Info --A53
   on BA_Info.Bond_Number = RA_Info.Bond_Number   
   Left Join Bond_Rating_Info oldA57 --oldA57
   on BA_Info.Bond_Number = oldA57.Bond_Number 
   AND BA_Info.Lots = oldA57.Lots 
   AND BA_Info.Portfolio_Name = oldA57.Portfolio_Name
   AND BA_Info.Origination_Date = oldA57.Origination_Date 
   AND oldA57.Rating_Type = '2'
   Left Join Grade_Mapping_Info GMapInfo --A52
   on RA_Info.Rating_Org = GMapInfo.Rating_Org
   AND oldA57.Rating = GMapInfo.Rating
   Left Join Grade_Moody_Info GMooInfo --A51
   on GMapInfo.PD_Grade = GMooInfo.PD_Grade
   Left Join Rating_Info_SampleInfo RISI --A53 Sample
   on BA_Info.Bond_Number = RISI.Bond_Number
   AND RA_Info.Rating_Object = '債項'
   Where BA_Info.Report_Date = '{reportData}'
   And   BA_Info.Version = {ver}
)
Insert into Bond_Rating_Info
           (Reference_Nbr,
           Bond_Number,
           Lots,
           Portfolio,
           Segment_Name,
           Bond_Type,
           Lien_position,
           Origination_Date,
           Report_Date,
           Rating_Date,
           Rating_Type,
           Rating_Object,
           Rating_Org,
           Rating,
           Rating_Org_Area,
           PD_Grade,
           Grade_Adjust,
           ISSUER_TICKER,
           GUARANTOR_NAME,
           GUARANTOR_EQY_TICKER,
           Parm_ID,
           Portfolio_Name,
           RTG_Bloomberg_Field,
           SMF,
           ISSUER,
           Version,
           Security_Ticker)
Select     Reference_Nbr,
		   Bond_Number,
		   Lots,
           Portfolio,
           Segment_Name,
           Bond_Type,
           Lien_position,
           Origination_Date,
           Report_Date,
           Rating_Date,
           '2',
           Rating_Object,
           Rating_Org,
           Rating,
           Rating_Org_Area,
           PD_Grade,
           Grade_Adjust,
           ISSUER_TICKER,
           GUARANTOR_NAME,
           GUARANTOR_EQY_TICKER,
           '{parmID}',
           Portfolio_Name,
           RTG_Bloomberg_Field,
           SMF,
           ISSUER,
           Version,
           Security_Ticker
		   From
		   T1;
Insert Into Bond_Rating_Summary
            (
			  Reference_Nbr,
              Report_Date,
              Parm_ID,
              Bond_Type,
              Rating_Type,
              Rating_Object,
              Rating_Org_Area,
              Rating_Selection,
              Grade_Adjust,
              Rating_Priority,
              --Processing_Date,
              Version,
              Bond_Number,
              Lots,
              Portfolio,
              Origination_Date,
              Portfolio_Name,
              SMF,
              ISSUER
			)
Select BR_Info.Reference_Nbr,
       BR_Info.Report_Date,
	   BR_Info.Parm_ID,
	   BR_Info.Bond_Type,
	   BR_Info.Rating_Type,
	   BR_Info.Rating_Object,
	   BR_Info.Rating_Org_Area,	   
	   BR_Parm.Rating_Selection,
	   (CASE WHEN BR_Parm.Rating_Selection = '1' 
	         THEN Min(BR_Info.Grade_Adjust)
			 WHEN BR_Parm.Rating_Selection = '2'
			 THEN Max(BR_Info.Grade_Adjust)
			 ELSE null  END) AS Grade_Adjust,
	   BR_Parm.Rating_Priority,
	   BR_Info.Version,
	   BR_Info.Bond_Number,
	   BR_Info.Lots,
	   BR_Info.Portfolio,
	   BR_Info.Origination_Date,
	   BR_Info.Portfolio_Name,
	   BR_Info.SMF,
	   BR_Info.ISSUER
From   Bond_Rating_Info BR_Info
LEFT JOIN  Bond_Rating_Parm BR_Parm
on         BR_Info.Parm_ID = BR_Parm.Parm_ID
AND        BR_Info.Rating_Object = BR_Parm.Rating_Object
AND        BR_Info.Rating_Org_Area = BR_Parm.Rating_Org_Area
Where  BR_Info.Report_Date = '{reportData}'
AND    BR_Info.Version = {ver}
GROUP BY BR_Info.Reference_Nbr,
         BR_Info.Report_Date,
		 BR_Info.Bond_Type,
	     BR_Info.Rating_Type,
	     BR_Info.Rating_Object,
	     BR_Info.Rating_Org_Area,
		 BR_Info.Parm_ID,
		 BR_Info.Version,
		 BR_Info.Bond_Number,
		 BR_Info.Lots,
	     BR_Info.Portfolio,
	     BR_Info.Origination_Date,
	     BR_Info.Portfolio_Name,
	     BR_Info.SMF,
	     BR_Info.ISSUER,
		 BR_Parm.Rating_Selection,
		 BR_Parm.Rating_Priority;
End Try Begin Catch End Catch
                    ";

                    #endregion
                    try
                    {
                        db.Database.ExecuteSqlCommand(sql);
                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = Message_Type.save_Success.GetDescription();
                    }
                    catch (Exception ex)
                    {
                        result.DESCRIPTION = Message_Type
                                     .save_Fail.GetDescription(null,
                                     $"message: {ex.Message}" +
                                     $", inner message {ex.InnerException?.InnerException?.Message}");
                    }
                }
                else
                {
                    result.DESCRIPTION = Message_Type.transferError.GetDescription();
                }
            }
            if (result.RETURN_FLAG)
            {
                common.saveTransferCheck(
                    Table_Type.A57.ToString(),
                    true,
                    dt,
                    version,
                    startTime,
                    DateTime.Now);
                common.saveTransferCheck(
                    Table_Type.A58.ToString(),
                    true,
                    dt,
                    version,
                    startTime,
                    DateTime.Now);
            }
            else
            {
                common.saveTransferCheck(
                    Table_Type.A57.ToString(),
                    false,
                    dt,
                    version,
                    startTime,
                    DateTime.Now);
                common.saveTransferCheck(
                    Table_Type.A58.ToString(),
                    false,
                    dt,
                    version,
                    startTime,
                    DateTime.Now);
            }
            return result;
        }

        #endregion Save Db

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
                RATING_AS_OF_DATE_OVERRIDE = item.RATING_AS_OF_DATE_OVERRIDE,
                Rating_Type = item.Rating_Type
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
                Rating_Type = TypeTransfer.objToString(item[8]), //Rating_Type
                Issuer = TypeTransfer.objToString(item[9]), //Issuer
                Security_Ticker = TypeTransfer.objToString(item[10]), //Security_Ticker
                RATING_AS_OF_DATE_OVERRIDE = TypeTransfer.objToString(item[11]), //RATING_AS_OF_DATE_OVERRIDE
                ISSUER_TICKER = TypeTransfer.objToString(item[12]), //ISSUER_TICKER
                GUARANTOR_NAME = TypeTransfer.objToString(item[13]), //GUARANTOR_NAME
                GUARANTOR_EQY_TICKER = TypeTransfer.objToString(item[14]), //GUARANTOR_EQY_TICKER
                RTG_SP = TypeTransfer.objToString(item[15]), //債項_標普評等 (債項\ sp\國外)
                SP_EFF_DT = TypeTransfer.objToString(item[16]), //債項_標普評等日期
                RTG_TRC = TypeTransfer.objToString(item[17]), //債項_TRC 評等 (債項\ CW\國內)
                TRC_EFF_DT = TypeTransfer.objToString(item[18]), //債項_TRC 評等日期
                RTG_MOODY = TypeTransfer.objToString(item[19]), //債項_穆迪評等 (債項\ moody\國外)
                MOODY_EFF_DT = TypeTransfer.objToString(item[20]), //債項_穆迪評等日期
                RTG_FITCH = TypeTransfer.objToString(item[21]), //債項_惠譽評等 (債項\ Fitch\國外)
                FITCH_EFF_DT = TypeTransfer.objToString(item[22]), //債項_惠譽評等日期
                RTG_FITCH_NATIONAL = TypeTransfer.objToString(item[23]), //債項_惠譽國內評等 (債項\ Fitch(twn)\國內)
                RTG_FITCH_NATIONAL_DT = TypeTransfer.objToString(item[24]), //債項_惠譽國內評等日期
                RTG_SP_LT_FC_ISSUER_CREDIT = TypeTransfer.objToString(item[25]), //標普長期外幣發行人信用評等 (發行人\ sp\國外)
                RTG_SP_LT_FC_ISS_CRED_RTG_DT = TypeTransfer.objToString(item[26]), //標普長期外幣發行人信用評等日期
                RTG_SP_LT_LC_ISSUER_CREDIT = TypeTransfer.objToString(item[27]),//標普本國貨幣長期發行人信用評等 (發行人\ sp\國外)
                RTG_SP_LT_LC_ISS_CRED_RTG_DT = TypeTransfer.objToString(item[28]), //標普本國貨幣長期發行人信用評等日期
                RTG_MDY_ISSUER = TypeTransfer.objToString(item[29]), //穆迪發行人評等 (發行人\ moody\國外)
                RTG_MDY_ISSUER_RTG_DT = TypeTransfer.objToString(item[30]), //穆迪發行人評等日期
                RTG_MOODY_LONG_TERM = TypeTransfer.objToString(item[31]), //發行人_穆迪長期評等 (發行人\ moody\國外)
                RTG_MOODY_LONG_TERM_DATE = TypeTransfer.objToString(item[32]), //發行人_穆迪長期評等日期
                RTG_MDY_SEN_UNSECURED_DEBT = TypeTransfer.objToString(item[33]), //發行人_穆迪優先無擔保債務評等 (發行人\ moody\國外)
                RTG_MDY_SEN_UNSEC_RTG_DT = TypeTransfer.objToString(item[34]), //發行人_穆迪優先無擔保債務評等_日期
                RTG_MDY_FC_CURR_ISSUER_RATING = TypeTransfer.objToString(item[35]), //穆迪外幣發行人評等 (發行人\ moody\國外)
                RTG_MDY_FC_CURR_ISSUER_RTG_DT = TypeTransfer.objToString(item[36]), //穆迪外幣發行人評等日期
                RTG_MDY_LOCAL_LT_BANK_DEPOSITS = TypeTransfer.objToString(item[37]), //發行人_穆迪長期本國銀行存款評等 (發行人\ moody\國內)
                RTG_MDY_LT_LC_BANK_DEP_RTG_DT = TypeTransfer.objToString(item[38]), //發行人_穆迪長期本國銀行存款評等日期
                RTG_FITCH_LT_ISSUER_DEFAULT = TypeTransfer.objToString(item[39]), //惠譽長期發行人違約評等 (發行人\ Fitch\國外)
                RTG_FITCH_LT_ISSUER_DFLT_RTG_DT = TypeTransfer.objToString(item[40]), //惠譽長期發行人違約評等日期
                RTG_FITCH_SEN_UNSECURED = TypeTransfer.objToString(item[41]), //發行人_惠譽優先無擔保債務評等 (發行人\ Fitch\國外)
                RTG_FITCH_SEN_UNSEC_RTG_DT = TypeTransfer.objToString(item[42]), //發行人_惠譽優先無擔保債務評等日期
                RTG_FITCH_LT_FC_ISSUER_DEFAULT = TypeTransfer.objToString(item[43]), //惠譽長期外幣發行人違約評等 (發行人\ Fitch\國外)
                RTG_FITCH_LT_FC_ISS_DFLT_RTG_DT = TypeTransfer.objToString(item[44]),  //惠譽長期外幣發行人違約評等日期
                RTG_FITCH_LT_LC_ISSUER_DEFAULT = TypeTransfer.objToString(item[45]), //惠譽長期本國貨幣發行人違約評等 (發行人\ Fitch\國外)
                RTG_FITCH_LT_LC_ISS_DFLT_RTG_DT = TypeTransfer.objToString(item[46]), //惠譽長期本國貨幣發行人違約評等日期
                RTG_FITCH_NATIONAL_LT = TypeTransfer.objToString(item[47]), //發行人_惠譽國內長期評等 (發行人\ Fitch(twn)\國內)
                RTG_FITCH_NATIONAL_LT_DT = TypeTransfer.objToString(item[48]), //發行人_惠譽國內長期評等日期
                RTG_TRC_LONG_TERM = TypeTransfer.objToString(item[49]), //發行人_TRC 長期評等 (發行人\ CW\國內)
                RTG_TRC_LONG_TERM_RTG_DT = TypeTransfer.objToString(item[50]), //發行人_TRC 長期評等日期
                G_RTG_SP_LT_FC_ISSUER_CREDIT = TypeTransfer.objToString(item[51]), //標普長期外幣保證人信用評等 (保證人\ sp\國外)
                G_RTG_SP_LT_FC_ISS_CRED_RTG_DT = TypeTransfer.objToString(item[52]), //標普長期外幣保證人信用評等日期
                G_RTG_SP_LT_LC_ISSUER_CREDIT = TypeTransfer.objToString(item[53]), //標普本國貨幣長期保證人信用評等 (保證人\ sp\國外)
                G_RTG_SP_LT_LC_ISS_CRED_RTG_DT = TypeTransfer.objToString(item[54]), //標普本國貨幣長期保證人信用評等日期
                G_RTG_MDY_ISSUER = TypeTransfer.objToString(item[55]), //穆迪保證人評等 (保證人\ moody\國外)
                G_RTG_MDY_ISSUER_RTG_DT = TypeTransfer.objToString(item[56]), //穆迪保證人評等日期
                G_RTG_MOODY_LONG_TERM = TypeTransfer.objToString(item[57]), //保證人_穆迪長期評等 (保證人\ moody\國外)
                G_RTG_MOODY_LONG_TERM_DATE = TypeTransfer.objToString(item[58]), //保證人_穆迪長期評等日期
                G_RTG_MDY_SEN_UNSECURED_DEBT = TypeTransfer.objToString(item[59]), //保證人_穆迪優先無擔保債務評等 (保證人\ moody\國外)
                G_RTG_MDY_SEN_UNSEC_RTG_DT = TypeTransfer.objToString(item[60]), //保證人_穆迪優先無擔保債務評等_日期
                G_RTG_MDY_FC_CURR_ISSUER_RATING = TypeTransfer.objToString(item[61]), //穆迪外幣保證人評等 (保證人\ moody\國外)
                G_RTG_MDY_FC_CURR_ISSUER_RTG_DT = TypeTransfer.objToString(item[62]), //穆迪外幣保證人評等日期
                G_RTG_MDY_LOCAL_LT_BANK_DEPOSITS = TypeTransfer.objToString(item[63]), //保證人_穆迪長期本國銀行存款評等 (保證人\ moody\國內)
                G_RTG_MDY_LT_LC_BANK_DEP_RTG_DT = TypeTransfer.objToString(item[64]), //保證人_穆迪長期本國銀行存款評等日期
                G_RTG_FITCH_LT_ISSUER_DEFAULT = TypeTransfer.objToString(item[65]), //惠譽長期保證人違約評等 (保證人\ Fitch\國外)
                G_RTG_FITCH_LT_ISSUER_DFLT_RTG_DT = TypeTransfer.objToString(item[66]), //惠譽長期保證人違約評等日期
                G_RTG_FITCH_SEN_UNSECURED = TypeTransfer.objToString(item[67]), //保證人_惠譽優先無擔保債務評等 (保證人\ Fitch\國外)
                G_RTG_FITCH_SEN_UNSEC_RTG_DT = TypeTransfer.objToString(item[68]), //保證人_惠譽優先無擔保債務評等日期
                G_RTG_FITCH_LT_FC_ISSUER_DEFAULT = TypeTransfer.objToString(item[69]), //惠譽長期外幣保證人違約評等 (保證人\ Fitch\國外)
                G_RTG_FITCH_LT_FC_ISS_DFLT_RTG_DT = TypeTransfer.objToString(item[70]), //惠譽長期外幣保證人違約評等日期
                G_RTG_FITCH_LT_LC_ISSUER_DEFAULT = TypeTransfer.objToString(item[71]), //惠譽長期本國貨幣保證人違約評等 (保證人\ Fitch\國外)
                G_RTG_FITCH_LT_LC_ISS_DFLT_RTG_DT = TypeTransfer.objToString(item[72]), //惠譽長期本國貨幣保證人違約評等日期
                G_RTG_FITCH_NATIONAL_LT = TypeTransfer.objToString(item[73]), //保證人_惠譽國內長期評等 (保證人\ Fitch(twn)\國內)
                G_RTG_FITCH_NATIONAL_LT_DT = TypeTransfer.objToString(item[74]), //保證人_惠譽國內長期評等日期
                G_RTG_TRC_LONG_TERM = TypeTransfer.objToString(item[75]), //保證人_TRC 長期評等 (保證人\ CW\國內)
                G_RTG_TRC_LONG_TERM_RTG_DT = TypeTransfer.objToString(item[76]), //保證人_TRC 長期評等日期
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rating"></param>
        /// <returns></returns>
        private string forRating(string rating)
        {
            if (rating.IsNullOrWhiteSpace())
                return string.Empty;
            string value = rating.Trim();
            if (value.IndexOf("u") > -1)
                return value.Split('u')[0].Trim();
            //====================================== 待確認
            if (value.IndexOf("NR") > -1)
                return string.Empty;
            if (value.IndexOf("twNR") > -1)
                return string.Empty;
            if (value.IndexOf("WD") > -1)
                return string.Empty;
            if (value.IndexOf("WR") > -1)
                return string.Empty;
            if (value.IndexOf("twWR") > -1)
                return string.Empty;
            //======================================
            if (value.IndexOf("/*-") > -1)
                return value.Split(new string[] { "/*-" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
            if (value.IndexOf("/*+") > -1)
                return value.Split(new string[] { "/*+" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
            if (value.IndexOf("*-") > -1)
                return value.Split(new string[] { "*-" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
            if (value.IndexOf("*+") > -1)
                return value.Split(new string[] { "*+" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
            return value;
        }

        /// <summary>
        /// 判斷國內外
        /// </summary>
        /// <param name="ratingOrg"></param>
        /// <returns></returns>
        private string formatOrgArea(string ratingOrg)
        {
            if (ratingOrg.Equals(RatingOrg.SP.GetDescription()) ||
               ratingOrg.Equals(RatingOrg.Moody.GetDescription()) ||
               ratingOrg.Equals(RatingOrg.Fitch.GetDescription()))
                return "國外";
            if (ratingOrg.Equals(RatingOrg.FitchTwn.GetDescription()) ||
                ratingOrg.Equals(RatingOrg.CW.GetDescription()))
                return "國內";
            return string.Empty;
        }


        /// <summary>
        /// get ParmID
        /// </summary>
        /// <returns></returns>
        private string getParmID()
        {
            string parmID = string.Empty;
            DateTime dt = DateTime.Now;
            var parms = db.Bond_Rating_Parm
            .Where(j => j.Processing_Date != null &&
                   dt > j.Processing_Date);
            Bond_Rating_Parm parmf =
            parms.FirstOrDefault(q => q.Processing_Date == parms.Max(w => w.Processing_Date));
            if (parmf != null) // 參數編號
                parmID = parmf.Parm_ID;
            return parmID;
        }

        /// <summary>
        /// 抓取 sampleInfo
        /// </summary>
        /// <param name="sampleInfos"></param>
        /// <param name="info"></param>
        /// <param name="nullarr"></param>
        /// <returns></returns>
        private Rating_Info_SampleInfo formateSampleInfo(
            List<Rating_Info_SampleInfo> sampleInfos,
            Rating_Info info,
            List<string> nullarr)
        {
            if (RatingObject.Bonds.GetDescription().Equals(info.Rating_Object))
            {
                Rating_Info_SampleInfo s = sampleInfos.FirstOrDefault(j => info.Bond_Number.Equals(j.Bond_Number));
                if (s != null)
                {
                    return new Rating_Info_SampleInfo()
                    {
                        Bond_Number = s.Bond_Number,
                        ISSUER_TICKER = sampleInfoValue(s.ISSUER_TICKER, nullarr),
                        GUARANTOR_EQY_TICKER = sampleInfoValue(s.GUARANTOR_EQY_TICKER, nullarr),
                        GUARANTOR_NAME = sampleInfoValue(s.GUARANTOR_NAME, nullarr)
                    };
                }
            }
            return new Rating_Info_SampleInfo();
        }

        /// <summary>
        /// 判斷sampleInfo 參數
        /// </summary>
        /// <param name="value"></param>
        /// <param name="nullarr"></param>
        /// <returns></returns>
        private string sampleInfoValue(string value, List<string> nullarr)
        {
            if (value == null)
                return null;
            if (nullarr.Contains(value.Trim()))
                return null;
            return value.Trim();
        }

        #endregion Private Function
    }
}