using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Transfer.Models;
using Transfer.Utility;
using Transfer.ViewModels;
using static Transfer.Enum.Ref;
using System.Configuration;

namespace Transfer.Controllers
{
    public class IFRS9Controller : Controller
    {
        private IFRS9SecondEntities db = new IFRS9SecondEntities();

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 選擇檔案後點選資料上傳觸發
        /// </summary>
        /// <param name="FileModel"></param>
        /// <returns>MSGReturnModel</returns>
        [HttpPost]
        public JsonResult Upload(ValidateFiles FileModel)
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                #region 前端無傳送檔案進來
                if (FileModel.File == null)
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = "請選擇檔案!";
                    return Json(result);
                }
                #endregion

                #region 前端檔案大小不服或不為Excel檔案(驗證)
                if (FileModel.File.ContentLength == 0 || !ModelState.IsValid)
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = "請確認檔案為Excel檔案或超過大小!";
                    return Json(result);
                }
                #endregion

                #region 上傳檔案
                var fileName = Path.GetFileName(FileModel.File.FileName); //檔案名稱

                #region 檢查是否有FileUploads資料夾,如果沒有就新增 並加入 excel 檔案

                string projectFile = Server.MapPath("~/FileUploads"); //專案資料夾
                string path = Path.Combine(projectFile, fileName);
                createFile(projectFile);

                using (var fileStream = new FileStream(path,
                    FileMode.Create, FileAccess.ReadWrite))
                {
                    FileModel.File.InputStream.CopyTo(fileStream);
                }
                #endregion

                #region 讀取Excel資料 使用ExcelDataReader 並且組成 json
                string pathType =
                    Path.GetExtension(FileModel.File.FileName)
                    .Substring(1); //檔案類型
                var stream = FileModel.File.InputStream;
                List<ExhibitModel> dataModel = getExcel(pathType, stream);
                if (dataModel.Count > 0)
                {
                    result.RETURN_FLAG = true;
                    result.Datas = Json(dataModel);
                }
                else
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = "無筆對到資料!";
                }
                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = ex.Message;
            }
            return Json(result);
        }

        [HttpPost]
        public JsonResult Transfer()
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                // Excel 檔案位置
                DateTime startTime = DateTime.Now;
                string projectFile = Server.MapPath("~/FileUploads");
                string fileName = @"Exhibit 10.xlsx";
                //string configFileName = ConfigurationManager.AppSettings["fileName"];
                
                string path = Path.Combine(projectFile, fileName);
                FileStream stream = System.IO.File.Open(path, FileMode.Open, FileAccess.Read);

                string pathType = path.Split('.')[1];
                List<ExhibitModel> dataModel = getExcel(pathType, stream);

                string proName = "Transfer";
                string tableName = string.Empty;

                tableName = "Moody_Monthly_PD_Info";
                bool flagA81 = saveA81(dataModel);
                bool A81Log = saveLog(tableName, fileName, proName, flagA81, startTime, DateTime.Now);
                txtLog(tableName, flagA81, startTime);

                tableName = "Moody_Quartly_PD_Info";
                bool flagA82 = saveA82(dataModel);
                bool A82Log = saveLog(tableName, fileName, proName, flagA82, startTime, DateTime.Now);
                txtLog(tableName, A82Log, startTime);

                tableName = "Moody_Predit_PD_Info";
                bool flagA83 = saveA83(dataModel);
                bool A83Log = saveLog(tableName, fileName, proName, flagA83, startTime, DateTime.Now);
                txtLog(tableName, A82Log, startTime);

                result.RETURN_FLAG = flagA81 && flagA82 && flagA83;
                if (!result.RETURN_FLAG)
                {
                    result.DESCRIPTION = (flagA81 ? string.Empty : "A81 Error! ") +
                                         (flagA82 ? string.Empty : "A82 Error! ") +
                                         (flagA83 ? string.Empty : "A83 Error! ");
                }
                else
                {
                    result.DESCRIPTION = "Success!";
                }
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = ex.Message;
            }
            return Json(result);
        }

        [HttpPost]
        public JsonResult GetData(string type)
        {
            MSGReturnModel result = new MSGReturnModel();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = "No Data !";
            try
            {
                switch (type)
                {
                    case "A81":
                        if (db.Moody_Monthly_PD_Info.Count() > 0)
                        {
                            result.RETURN_FLAG = true;
                            result.Datas = Json(
                                (from item in db.Moody_Monthly_PD_Info.AsEnumerable()
                                 select new A81ViewModel()
                                {
                                     Trailing_12m_Ending =
                                    item.Trailing_12m_Ending.HasValue ?
                                    item.Trailing_12m_Ending.Value.ToString("yyyy/MM/dd") : string.Empty,
                                    Actual_Allcorp = doubleToString(item.Actual_Allcorp),
                                    Actual_SG = doubleToString(item.Actual_SG),
                                    Baseline_forecast_Allcorp = doubleToString(item.Baseline_forecast_Allcorp),
                                    Baseline_forecast_SG = doubleToString(item.Baseline_forecast_SG),
                                    Pessimistic_Forecast_Allcorp = doubleToString(item.Pessimistic_Forecast_Allcorp),
                                    Pessimistic_Forecast_SG = doubleToString(item.Pessimistic_Forecast_SG),
                                    Data_Year = item.Data_Year
                                }).ToList());
                        }
                        break;
                    case "A82":
                        if (db.Moody_Quartly_PD_Info.Count() > 0)
                        {
                            result.RETURN_FLAG = true;
                            result.Datas = Json(db.Moody_Quartly_PD_Info.ToList());
                        }
                        break;
                    case "A83":
                        if (db.Moody_Predit_PD_Info.Count() > 0)
                        {
                            result.RETURN_FLAG = true;
                            result.Datas = Json(db.Moody_Predit_PD_Info.ToList());
                        }
                        break;
                }
                if (result.RETURN_FLAG)
                    result.DESCRIPTION = "Success!";
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = ex.Message;
            }
            return Json(result);
        }

        #region private function

        #region save A81
        private bool saveA81(List<ExhibitModel> dataModel)
        {
            bool flag = true;
            try
            {
                foreach (var item in db.Moody_Monthly_PD_Info)
                {
                    db.Moody_Monthly_PD_Info.Remove(item);
                }
                int id = 1;
                foreach (var item in dataModel)
                {
                    DateTime? dt = null;
                    if (!string.IsNullOrWhiteSpace(item.Trailing))
                        dt = DateTime.Parse(item.Trailing);
                    double? actualAllcorp = null;
                    if (!string.IsNullOrWhiteSpace(item.Actual_Allcorp))
                        actualAllcorp = double.Parse(item.Actual_Allcorp);
                    double? baselineForecastAllcorp = null;
                    if (!string.IsNullOrWhiteSpace(item.Baseline_forecast_Allcorp))
                        baselineForecastAllcorp = double.Parse(item.Baseline_forecast_Allcorp);
                    double? pessimisticForecastAllcorp = null;
                    if (!string.IsNullOrWhiteSpace(item.Pessimistic_Forecast_Allcorp))
                        pessimisticForecastAllcorp = double.Parse(item.Pessimistic_Forecast_Allcorp);
                    double? actualSG = null;
                    if (!string.IsNullOrWhiteSpace(item.Actual_SG))
                        actualSG = double.Parse(item.Actual_SG);
                    double? baselineForecastSG = null;
                    if (!string.IsNullOrWhiteSpace(item.Baseline_forecast_SG))
                        baselineForecastSG = double.Parse(item.Baseline_forecast_SG);
                    double? pessimisticForecastSG = null;
                    if (!string.IsNullOrWhiteSpace(item.Pessimistic_Forecast_SG))
                        pessimisticForecastSG = double.Parse(item.Pessimistic_Forecast_SG);
                    db.Moody_Monthly_PD_Info.Add(
                        new Moody_Monthly_PD_Info()
                        {
                            Id = id,
                            Trailing_12m_Ending = dt,
                            Actual_Allcorp = actualAllcorp,
                            Baseline_forecast_Allcorp = baselineForecastAllcorp,
                            Pessimistic_Forecast_Allcorp = pessimisticForecastAllcorp,
                            Actual_SG = actualSG,
                            Baseline_forecast_SG = baselineForecastSG,
                            Pessimistic_Forecast_SG = pessimisticForecastSG,
                            Data_Year = (dt == null) ? string.Empty : ((DateTime)dt).Year.ToString()
                        });
                    id += 1;
                }

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                foreach (var item in db.Moody_Monthly_PD_Info)
                {
                    db.Moody_Monthly_PD_Info.Remove(item);
                }
                flag = false;
            }
            return flag;
        }
        #endregion

        #region save A82
        private bool saveA82(List<ExhibitModel> dataModel)
        {
            bool flag = true;
            try
            {
                foreach (var item in db.Moody_Quartly_PD_Info)
                {
                    db.Moody_Quartly_PD_Info.Remove(item);
                }
                int id = 1;
                List<Moody_Quartly_PD_Info> allData = new List<Moody_Quartly_PD_Info>();
                List<int> months = new List<int>() { 3, 6, 9, 12 };
                foreach (var item in dataModel
                    .Where(x => !string.IsNullOrWhiteSpace(x.Actual_Allcorp)
                    && months.Contains(DateTime.Parse(x.Trailing).Month))
                    .OrderByDescending(x => x.Trailing))
                {
                    DateTime dt = DateTime.Parse(item.Trailing);
                    string quartly = dt.Year.ToString();
                    switch (dt.Month)
                    {
                        case 3:
                            quartly += "Q1";
                            break;
                        case 6:
                            quartly += "Q2";
                            break;
                        case 9:
                            quartly += "Q3";
                            break;
                        case 12:
                            quartly += "Q4";
                            break;
                    }
                    double? actualAllcorp = null;
                    if (!string.IsNullOrWhiteSpace(item.Actual_Allcorp))
                        actualAllcorp = double.Parse(item.Actual_Allcorp);
                    db.Moody_Quartly_PD_Info.Add(new Moody_Quartly_PD_Info()
                    {
                        Id = id,
                        Data_Year = dt.Year.ToString(),
                        Year_Quartly = quartly,
                        PD = actualAllcorp
                    });
                    id += 1;
                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                foreach (var item in db.Moody_Quartly_PD_Info)
                {
                    db.Moody_Quartly_PD_Info.Remove(item);
                }
                flag = false;
            }
            return flag;
        }
        #endregion

        #region save A83
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        private bool saveA83(List<ExhibitModel> dataModel)
        {
            bool flag = true;
            try
            {
                foreach (var item in db.Moody_Predit_PD_Info)
                {
                    db.Moody_Predit_PD_Info.Remove(item);
                }
                List<ExhibitModel> models = (from q in dataModel
                                             where !string.IsNullOrWhiteSpace(q.Actual_Allcorp) && //排除掉今年
                                             12.Equals(DateTime.Parse(q.Trailing).Month) //只取12月
                                             select q).ToList();
                string maxYear = models.Max(x => DateTime.Parse(x.Trailing)).Year.ToString();
                string minYear = models.Min(x => DateTime.Parse(x.Trailing)).Year.ToString();

                double? PD = null;
                double PDValue = models.Sum(x => double.Parse(x.Actual_Allcorp)) / models.Count;
                if (PDValue > 0)
                    PD = PDValue;

                //int id = 1;
                db.Moody_Predit_PD_Info.Add(new Moody_Predit_PD_Info()
                {
                    Id = 1,
                    Data_Year = maxYear,
                    Period = minYear + "-" + maxYear,
                    PD_TYPE = PD_Type.Past_Year_AVG.ToString(),
                    PD = PD
                });
                var dtn = DateTime.Now.Year;
                ExhibitModel model =
                    dataModel.Where(x => dtn.Equals(DateTime.Parse(x.Trailing).Year)
                    && 12.Equals(DateTime.Parse(x.Trailing).Month)).FirstOrDefault();
                string baselineForecastAllcorp = string.Empty;
                if (model != null)
                    baselineForecastAllcorp = model.Baseline_forecast_Allcorp;
                PD = null;
                if (!string.IsNullOrWhiteSpace(baselineForecastAllcorp))
                    PD = double.Parse(baselineForecastAllcorp);

                db.Moody_Predit_PD_Info.Add(new Moody_Predit_PD_Info()
                {
                    Id = 2,
                    Data_Year = maxYear,
                    Period = dtn.ToString(),
                    PD_TYPE = PD_Type.Forcast.ToString(),
                    PD = PD
                });
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                foreach (var item in db.Moody_Predit_PD_Info)
                {
                    db.Moody_Predit_PD_Info.Remove(item);
                }
                flag = false;
            }
            return flag;
        }
        #endregion

        #region save txtlog
        /// <summary>
        /// 寫入 Txt Log
        /// </summary>
        /// <param name="tableName">table名</param>
        /// <param name="falg">成功或失敗</param>
        /// <param name="start">開始時間</param>
        private void txtLog(string tableName, bool falg, DateTime start)
        {
            try
            {
                string projectFile = Server.MapPath("~/FileUploads"); //預設txt位置
                string configTxtLocation = ConfigurationManager.AppSettings["txtLogLocation"];
                if (!string.IsNullOrWhiteSpace(configTxtLocation))
                    projectFile = configTxtLocation; //有設定webConfig且不為空就取代
                createFile(projectFile);
                string path = "ExhibitTransfer.txt"; //預設txt名稱
                string configTxtName = ConfigurationManager.AppSettings["txtLogName"];
                if (!string.IsNullOrWhiteSpace(configTxtName))
                    path = configTxtName; //有設定webConfig且不為空就取代
                string folderPath = Path.Combine(projectFile, path); //合併路徑&檔名
                string txtData = string.Empty;
                try //試著抓取舊資料
                {
                    txtData = System.IO.File.ReadAllText(folderPath);
                }
                catch { }
                string txt = string.Format("{0}_{1}_{2}",
                             tableName,
                             start.ToString("yyyyMMddHHmmss"),
                             falg ? "Y" : "N");
                if (!string.IsNullOrWhiteSpace(txtData)) //有舊資料就換行寫入下一筆
                {
                    txtData += string.Format("\r\n{0}", txt);
                }
                else //沒有就直接寫入
                {
                    txtData = txt;
                }
                FileStream fs = new FileStream(folderPath, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);
                sw.Write(txtData); //存檔
                sw.Close(); 
            }
            catch (Exception ex)
            {
                string a = ex.Message;
            }
        }
        #endregion

        #region save sqllog
        /// <summary>
        /// Log資料存到Sql
        /// </summary>
        /// <param name="tableName">table名</param>
        /// <param name="fileName">檔案名</param>
        /// <param name="programName">專案名</param>
        /// <param name="falg">成功失敗</param>
        /// <param name="start">開始時間</param>
        /// <param name="end">結束時間</param>
        /// <returns>回傳成功或失敗</returns>
        private bool saveLog(
            string tableName,
            string fileName,
            string programName,
            bool falg,
            DateTime start,
            DateTime end)
        {
            bool flag = true;
            try
            {
                int id = 1;
                if (db.IFRS9_Log.Count() > 0) //判斷有無舊的Log
                {
                    id += db.IFRS9_Log.Max(x => x.Id); //Id(Pk) 加一
                }
                db.IFRS9_Log.Add(new IFRS9_Log() //寫入DB
                {
                    Id = id,
                    Table_name = tableName.Substring(0, 20),
                    File_name = fileName,
                    Program_name = programName,
                    Create_date = start.ToString("yyyyMMdd"),
                    Create_time = start.ToString("HHmmss"),
                    End_date = end.ToString("yyyyMMdd"),
                    End_time = end.ToString("HHmmss"),
                    TYPE = falg ? "Y" : "N"
                });
                db.SaveChanges(); //DB SAVE
            }
            catch (Exception ex)
            {
                flag = false;
            }
            return flag;
        }
        #endregion

        #region datarow 組成 ExhibitModel
        /// <summary>
        /// datarow 組成 ExhibitModel
        /// </summary>
        /// <param name="item">DataRow</param>
        /// <returns>ExhibitModel</returns>
        private ExhibitModel getExhibitModels(DataRow item)
        {
            DateTime minDate = DateTime.MinValue;
            if (item[0] != null)
                DateTime.TryParse(item[0].ToString(), out minDate);
            return new ExhibitModel()
            {
                Trailing = (item[0] != null) && (minDate != DateTime.MinValue) ?
                minDate.ToString("yyyy/MM/dd") : string.Empty,
                Actual_Allcorp = (item[1] != null) ?
                item[1].ToString() : string.Empty,
                Baseline_forecast_Allcorp = (item[2] != null) ?
                item[2].ToString() : string.Empty,
                Pessimistic_Forecast_Allcorp = (item[3] != null) ?
                item[3].ToString() : string.Empty,
                Actual_SG = (item[4] != null) ?
                item[4].ToString() : string.Empty,
                Baseline_forecast_SG = (item[5] != null) ?
                item[5].ToString() : string.Empty,
                Pessimistic_Forecast_SG = (item[6] != null) ?
                item[6].ToString() : string.Empty
            };
        }
        #endregion

        #region get Excel to List<ExhibitModel>
        /// <summary>
        /// 把Excel 資料轉換成 ExhibitModel
        /// </summary>
        /// <param name="pathType">string</param>
        /// <param name="stream">Stream</param>
        /// <returns>ExhibitModels</returns>
        private List<ExhibitModel> getExcel(string pathType, Stream stream)
        {
            DataSet resultData = new DataSet();
            List<ExhibitModel> dataModel = new List<ExhibitModel>();
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
                                 select getExhibitModels(q)).Skip(1).ToList();
                                 //skip(1) 為排除Excel Title列那行(參數可調)
                }
            }
            catch
            { }
            return dataModel;
        }
        #endregion

        #region Create 資料夾
        /// <summary>
        /// Create 資料夾(判斷如果沒有的話就新增)
        /// </summary>
        /// <param name="projectFile">資料夾位置</param>
        private void createFile(string projectFile)
        {
            bool exists = Directory.Exists(projectFile);
            if (!exists) Directory.CreateDirectory(projectFile);
        }
        #endregion

        #region Double? To String
        /// <summary>
        /// Double? 轉string (null 回傳 string.Empty)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string doubleToString(double? value)
        {
            if (value.HasValue)
                return value.Value.ToString();
            return string.Empty;
        }
        #endregion

        #endregion


    }
}