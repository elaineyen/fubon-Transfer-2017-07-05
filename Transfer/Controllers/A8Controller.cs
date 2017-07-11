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
    public class A8Controller : Controller
    {
        private IFRS9Entities db = new IFRS9Entities();

        /// <summary>
        /// A8(上傳檔案)
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.Manu = "A8Main";
            ViewBag.SubManu = "A80SubMain";
            return View();
        }

        /// <summary>
        /// A8(查詢(A81.A82.A83))
        /// </summary>
        /// <returns></returns>
        public ActionResult Detail()
        {
            ViewBag.Manu = "A8Main";
            ViewBag.SubManu = "A81SubMain";
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

                FileUpLoad.createFile(projectFile); //檢查是否有FileUploads資料夾,如果沒有就新增

                //呼叫上傳檔案 function
                result = FileUpLoad.FileUpLoadinPath(path, FileModel.File);

                if (!result.RETURN_FLAG)
                    return Json(result);
                #endregion

                #region 讀取Excel資料 使用ExcelDataReader 並且組成 json
                string pathType =
                    Path.GetExtension(FileModel.File.FileName)
                    .Substring(1); //檔案類型
                var stream = FileModel.File.InputStream;
                List<Exhibit10Model> dataModel = getExcel(pathType, stream);
                if (dataModel.Count > 0)
                {
                    result.RETURN_FLAG = true;
                    result.Datas = Json(dataModel); //給JqGrid 顯示
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

        /// <summary>
        /// 轉檔把Excel 資料存到 DB
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Transfer()
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                #region 抓Excel檔案 轉成 model
                // Excel 檔案位置
                DateTime startTime = DateTime.Now;
                string projectFile = Server.MapPath("~/FileUploads");
                string fileName = @"Exhibit 10.xlsx"; //預設
                string configFileName = ConfigurationManager.AppSettings["fileA8Name"];
                if (!string.IsNullOrWhiteSpace(configFileName))
                    fileName = configFileName; //config 設定就取代
                string path = Path.Combine(projectFile, fileName);
                FileStream stream = System.IO.File.Open(path, FileMode.Open, FileAccess.Read);

                string pathType = path.Split('.')[1]; //抓副檔名
                List<Exhibit10Model> dataModel = getExcel(pathType, stream); //Excel轉成 Exhibit10Model

                string proName = "Transfer";
                string tableName = string.Empty;
                #endregion

                #region save Moody_Monthly_PD_Info(A81)
                tableName = "Moody_Monthly_PD_Info";
                bool flagA81 = saveA81(dataModel); //save to DB
                bool A81Log = saveLog(tableName, fileName, proName, flagA81, startTime, DateTime.Now); //寫sql Log
                TxtLog.txtLog(tableName, flagA81, startTime, txtLocation()); //寫txt Log
                #endregion

                #region save Moody_Quartly_PD_Info(A82)
                tableName = "Moody_Quartly_PD_Info";
                bool flagA82 = saveA82(dataModel); //save to DB
                bool A82Log = saveLog(tableName, fileName, proName, flagA82, startTime, DateTime.Now); //寫sql Log
                TxtLog.txtLog(tableName, A82Log, startTime, txtLocation()); //寫txt Log
                #endregion

                #region save Moody_Predit_PD_Info(A83)
                tableName = "Moody_Predit_PD_Info";
                bool flagA83 = saveA83(dataModel); //save to DB
                bool A83Log = saveLog(tableName, fileName, proName, flagA83, startTime, DateTime.Now); //寫sql Log
                TxtLog.txtLog(tableName, A82Log, startTime, txtLocation()); //寫txt Log
                #endregion

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

        /// <summary>
        /// 前端抓資料時呼叫
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetData(string type)
        {
            MSGReturnModel result = new MSGReturnModel();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = "No Data!";
            try
            {
                switch (type)
                {
                    case "A81": //抓Moody_Monthly_PD_Info(A81)資料
                        if (db.Moody_Monthly_PD_Info.Count() > 0)
                        {
                            result.RETURN_FLAG = true;
                            result.Datas = Json(
                                (from item in db.Moody_Monthly_PD_Info.AsEnumerable()
                                 select new A81ViewModel() //轉型 Datetime
                                {
                                     Trailing_12m_Ending =
                                    item.Trailing_12m_Ending.HasValue ?
                                    item.Trailing_12m_Ending.Value.ToString("yyyy/MM/dd") : string.Empty,
                                    Actual_Allcorp = TypeTransfer.doubleToString(item.Actual_Allcorp),
                                    Actual_SG = TypeTransfer.doubleToString(item.Actual_SG),
                                    Baseline_forecast_Allcorp = TypeTransfer.doubleToString(item.Baseline_forecast_Allcorp),
                                    Baseline_forecast_SG = TypeTransfer.doubleToString(item.Baseline_forecast_SG),
                                    Pessimistic_Forecast_Allcorp = TypeTransfer.doubleToString(item.Pessimistic_Forecast_Allcorp),
                                    Pessimistic_Forecast_SG = TypeTransfer.doubleToString(item.Pessimistic_Forecast_SG),
                                    Data_Year = item.Data_Year
                                }).ToList());
                        }
                        break;
                    case "A82"://抓Moody_Quartly_PD_Info(A82)資料
                        if (db.Moody_Quartly_PD_Info.Count() > 0)
                        {
                            result.RETURN_FLAG = true;
                            result.Datas = Json(db.Moody_Quartly_PD_Info.ToList());
                        }
                        break;
                    case "A83"://抓Moody_Predit_PD_Info(A83)資料
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

        #region save Moody_Monthly_PD_Info(A81)
        /// <summary>
        /// Save  Moody_Monthly_PD_Info(A81)
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        private bool saveA81(List<Exhibit10Model> dataModel)
        {
            bool flag = true;
            try
            {
                foreach (var item in db.Moody_Monthly_PD_Info)
                {
                    db.Moody_Monthly_PD_Info.Remove(item); //資料全刪除
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

                db.SaveChanges(); //Save
            }
            catch (Exception ex)
            {
                foreach (var item in db.Moody_Monthly_PD_Info)
                {
                    db.Moody_Monthly_PD_Info.Remove(item); //失敗先刪除
                }
                flag = false;
            }
            return flag;
        }
        #endregion

        #region save Moody_Quartly_PD_Info(A82)
        /// <summary>
        /// save Moody_Quartly_PD_Info(A82)
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        private bool saveA82(List<Exhibit10Model> dataModel)
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
                List<int> months = new List<int>() { 3, 6, 9, 12 }; //只搜尋3.6.9.12 月份
                foreach (var item in dataModel
                    .Where(x => !string.IsNullOrWhiteSpace(x.Actual_Allcorp) //要有Actual_Allcorp (排除今年)
                    && months.Contains(DateTime.Parse(x.Trailing).Month)) //只搜尋3.6.9.12 月份
                    .OrderByDescending(x => x.Trailing)) //排序=>日期大到小
                {
                    DateTime dt = DateTime.Parse(item.Trailing);
                    string quartly = dt.Year.ToString();
                    switch (dt.Month) //判斷季別
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

        #region save Moody_Predit_PD_Info(A83)
        /// <summary>
        /// save Moody_Predit_PD_Info(A83)
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        private bool saveA83(List<Exhibit10Model> dataModel)
        {
            bool flag = true;
            try
            {
                foreach (var item in db.Moody_Predit_PD_Info)
                {
                    db.Moody_Predit_PD_Info.Remove(item);
                }
                List<Exhibit10Model> models = (from q in dataModel
                                             where !string.IsNullOrWhiteSpace(q.Actual_Allcorp) && //排除掉今年
                                             12.Equals(DateTime.Parse(q.Trailing).Month) //只取12月
                                             select q).ToList();
                string maxYear = models.Max(x => DateTime.Parse(x.Trailing)).Year.ToString(); //抓取最大年
                string minYear = models.Min(x => DateTime.Parse(x.Trailing)).Year.ToString(); //抓取最小年

                double? PD = null;
                double PDValue = models.Sum(x => double.Parse(x.Actual_Allcorp)) / models.Count; //計算 PD
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
                Exhibit10Model model =
                    dataModel.Where(x => dtn.Equals(DateTime.Parse(x.Trailing).Year)
                    && 12.Equals(DateTime.Parse(x.Trailing).Month)).FirstOrDefault(); //抓今年又是12月的資料
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

        #region save sqllog(IFRS9_Log)
        /// <summary>
        /// Log資料存到Sql(IFRS9_Log)
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

        #region datarow 組成 Exhibit10Model
        /// <summary>
        /// datarow 組成 Exhibit10Model
        /// </summary>
        /// <param name="item">DataRow</param>
        /// <returns>Exhibit10Model</returns>
        private Exhibit10Model getExhibit10Models(DataRow item)
        {
            DateTime minDate = DateTime.MinValue;
            if (item[0] != null)
                DateTime.TryParse(item[0].ToString(), out minDate);
            return new Exhibit10Model()
            {
                Trailing = (item[0] != null) && (minDate != DateTime.MinValue) ?
                minDate.ToString("yyyy/MM/dd") : string.Empty,
                Actual_Allcorp = TypeTransfer.objToString(item[1]) ,
                Baseline_forecast_Allcorp = TypeTransfer.objToString(item[2]),
                Pessimistic_Forecast_Allcorp = TypeTransfer.objToString(item[3]) ,
                Actual_SG = TypeTransfer.objToString(item[4]) ,
                Baseline_forecast_SG = TypeTransfer.objToString(item[5]),
                Pessimistic_Forecast_SG = TypeTransfer.objToString(item[6])
            };
        }
        #endregion

        #region get Excel to List<Exhibit10Model>
        /// <summary>
        /// 把Excel 資料轉換成 Exhibit10Model
        /// </summary>
        /// <param name="pathType">string</param>
        /// <param name="stream">Stream</param>
        /// <returns>Exhibit10Models</returns>
        private List<Exhibit10Model> getExcel(string pathType, Stream stream)
        {
            DataSet resultData = new DataSet();
            List<Exhibit10Model> dataModel = new List<Exhibit10Model>();
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
                                 select getExhibit10Models(q)).Skip(1).ToList();
                    //skip(1) 為排除Excel Title列那行(參數可調)
                }
            }
            catch
            { }
            return dataModel;
        }
        #endregion

        #region txtlog 設定位置
        private string txtLocation()
        {
            try
            {
                string projectFile = Server.MapPath("~/FileUploads"); //預設txt位置
                string configTxtLocation = ConfigurationManager.AppSettings["txtLogLocation"];
                if (!string.IsNullOrWhiteSpace(configTxtLocation))
                    projectFile = configTxtLocation; //有設定webConfig且不為空就取代
                FileUpLoad.createFile(projectFile);
                string path = "ExhibitTransfer.txt"; //預設txt名稱
                string configTxtName = ConfigurationManager.AppSettings["txtLogName"];
                if (!string.IsNullOrWhiteSpace(configTxtName))
                    path = configTxtName; //有設定webConfig且不為空就取代
                string folderPath = Path.Combine(projectFile, path); //合併路徑&檔名
                return folderPath;
            }
            catch
            {
                return string.Empty;
            }
        }
        #endregion

        #endregion

    }
}