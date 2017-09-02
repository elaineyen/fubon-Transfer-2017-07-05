using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Transfer.Infrastructure;
using Transfer.Models.Interface;
using Transfer.Models.Repository;
using Transfer.Utility;
using Transfer.ViewModels;
using static Transfer.Enum.Ref;

namespace Transfer.Controllers
{
    [Authorize]
    public class A4Controller : CommonController
    {
        private IA4Repository A4Repository;
        private ICommon CommonFunction;
        private string[] selects = { "All", "B01", "C01" };
        private string[] selectsMortgage = { "All", "B01", "C01", "C02" };

        public A4Controller()
        {
            this.A4Repository = new A4Repository();
            this.CommonFunction = new Common();
            this.Cache = new DefaultCacheProvider();
        }

        public ICacheProvider Cache { get; set; }

        /// <summary>
        /// A4(上傳檔案)
        /// </summary>
        /// <returns></returns>
        [UserAuth("Index,A4")]
        public ActionResult Index()
        {
            var jqgridInfo = new A41ViewModel().TojqGridData();
            ViewBag.jqgridColNames = jqgridInfo.colNames;
            ViewBag.jqgridColModel = jqgridInfo.colModel;
            return View();
        }

        /// <summary>
        /// A41(債券明細檔)
        /// </summary>
        /// <returns></returns>
        [UserAuth("A41Detail,A4")]
        public ActionResult A41Detail()
        {
            var jqgridInfo = new A41ViewModel().TojqGridData();
            ViewBag.jqgridColNames = jqgridInfo.colNames;
            ViewBag.jqgridColModel = jqgridInfo.colModel;
            return View();
        }

        /// <summary>
        /// A42(國庫券月結資料檔)
        /// </summary>
        /// <returns></returns>
        [UserAuth("A42,A4")]
        public ActionResult A42()
        {
            return View();
        }

        /// <summary>
        /// 執行減損計算 (債券)
        /// </summary>
        /// <returns></returns>
        [UserAuth("A42Detail,A4")]
        public ActionResult A42Detail()
        {
            ViewBag.selectOption = new SelectList(
                selects.Select(x => new { Text = x, Value = x }), "Value", "Text");
            return View();
        }

        /// <summary>
        /// 執行減損計算 (房貸)
        /// </summary>
        /// <returns></returns>
        [UserAuth("A43Detail,A4")]
        public ActionResult A43Detail()
        {
            ViewBag.selectOption = new SelectList(
                selectsMortgage.Select(x => new { Text = x, Value = x }), "Value", "Text");

            return View();
        }

        /// <summary>
        /// Get Cache Data
        /// </summary>
        /// <param name="jdata"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata, string type)
        {
            List<A41ViewModel> data = new List<A41ViewModel>();
            switch (type)
            {
                case "Excel":
                    if (Cache.IsSet(CacheList.A41ExcelfileData))
                        data = (List<A41ViewModel>)Cache.Get(CacheList.A41ExcelfileData);  //從Cache 抓資料
                    break;

                case "Db":
                    if (Cache.IsSet(CacheList.A41DbfileData))
                        data = (List<A41ViewModel>)Cache.Get(CacheList.A41DbfileData);
                    break;
            }
            return Json(jdata.modelToJqgridResult(data));
        }

        /// <summary>
        /// /// 前端抓資料時呼叫
        /// </summary>
        /// <param name="type">A41</param>
        /// <param name="searchType">Report = 報導日資料查詢,Bonds = 債券資料查詢</param>
        /// <param name="value">版本 or 債券編號</param>
        /// <param name="date">報導日 or 債券購入日期</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetData(string type, string searchType, string value, string date)
        {
            MSGReturnModel result = new MSGReturnModel();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Message_Type.not_Find_Any.GetDescription(type);

            DateTime d = new DateTime();
            if (!DateTime.TryParse(date, out d))
            {
                result.DESCRIPTION = Message_Type.parameter_Error.GetDescription();
                return Json(result);
            }

            try
            {
                switch (type)
                {
                    case "A41":
                        var A41Data = A4Repository.GetA41(searchType, value, d);
                        result.RETURN_FLAG = A41Data.Item1;
                        Cache.Invalidate(CacheList.A41DbfileData); //清除
                        Cache.Set(CacheList.A41DbfileData, A41Data.Item2, 15); //把資料存到 Cache
                        if (!result.RETURN_FLAG)
                            result.DESCRIPTION = Message_Type.not_Find_Any.GetDescription();
                        break;
                }
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type.not_Find_Any.GetDescription(type, ex.Message);
            }
            return Json(result);
        }

        /// <summary>
        /// 抓取資料庫最後一天日期log Data
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetLogData(string debt)
        {
            if (Debt_Type.M.ToString().Equals(debt))
            {
                selects = selectsMortgage;
            }
            List<string> logDatas = A4Repository.GetLogData(selects.ToList(), debt);
            return Json(string.Join(",", logDatas));
        }



        /// <summary>
        /// 轉檔把Excel 資料存到 DB
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Transfer(string reportDate, string version)
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                #region 抓Excel檔案 轉成 model

                // Excel 檔案位置
                DateTime startTime = DateTime.Now;
                string projectFile = Server.MapPath("~/" + SetFile.FileUploads);

                string fileName = string.Empty;
                if (Cache.IsSet(CacheList.A41ExcelName))
                    fileName = (string)Cache.Get(CacheList.A41ExcelName);  //從Cache 抓資料

                if (fileName.IsNullOrWhiteSpace())
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = Message_Type.time_Out.GetDescription();
                }

                string path = Path.Combine(projectFile, fileName);
                FileStream stream = System.IO.File.Open(path, FileMode.Open, FileAccess.Read);

                string pathType = path.Split('.')[1]; //抓副檔名
                List<A41ViewModel> dataModel = A4Repository.getExcel(pathType, stream); //Excel轉成 Exhibit10Model

                #endregion 抓Excel檔案 轉成 model

                #region txtlog 檔案名稱

                string txtpath = SetFile.A41TransferTxtLog; //預設txt名稱
                string configTxtName = ConfigurationManager.AppSettings["txtLogA4Name"];
                if (!string.IsNullOrWhiteSpace(configTxtName))
                    txtpath = configTxtName; //有設定webConfig且不為空就取代

                #endregion txtlog 檔案名稱

                #region save Bond_Account_Info(A41)

                MSGReturnModel resultA41 = A4Repository.saveA41(dataModel, reportDate, version); //save to DB
                bool A41Log = CommonFunction.saveLog(Table_Type.A41,
                    fileName, SetFile.ProgramName, resultA41.RETURN_FLAG,
                    Debt_Type.B.ToString(), startTime, DateTime.Now); //寫sql Log
                TxtLog.txtLog(Table_Type.A41, resultA41.RETURN_FLAG, startTime, txtLocation(txtpath)); //寫txt Log

                #endregion save Bond_Account_Info(A41)

                result.RETURN_FLAG = resultA41.RETURN_FLAG;
                result.DESCRIPTION = Message_Type.save_Success.GetDescription(Table_Type.A41.ToString());

                if (!result.RETURN_FLAG)
                {
                    result.DESCRIPTION = Message_Type.save_Fail
                        .GetDescription(Table_Type.A41.ToString(), resultA41.DESCRIPTION);
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
        /// 轉檔把Excel 資料存到 DB
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult TransferA42(string processingDate, string reportDate)
        {
            MSGReturnModel result = new MSGReturnModel();

            try
            {
                #region 抓Excel檔案 轉成 model

                // Excel 檔案位置
                DateTime startTime = DateTime.Now;
                string projectFile = Server.MapPath("~/" + SetFile.FileUploads);
                string fileName = string.Empty;
                if (Cache.IsSet(CacheList.A42ExcelName))
                    fileName = (string)Cache.Get(CacheList.A42ExcelName);  //從Cache 抓資料

                if (fileName.IsNullOrWhiteSpace())
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = Message_Type.time_Out.GetDescription();
                }

                string path = Path.Combine(projectFile, fileName);
                FileStream stream = System.IO.File.Open(path, FileMode.Open, FileAccess.Read);

                string pathType = path.Split('.')[1]; //抓副檔名
                List<A42ViewModel> dataModel = A4Repository.getA42Excel(pathType, stream, processingDate, reportDate); //Excel轉成 A42ViewModel

                #endregion 抓Excel檔案 轉成 model

                #region txtlog 檔案名稱

                string txtpath = SetFile.A42TransferTxtLog; //預設txt名稱
                string configTxtName = ConfigurationManager.AppSettings["txtLogA42Name"];
                if (!string.IsNullOrWhiteSpace(configTxtName))
                {
                    txtpath = configTxtName; //有設定webConfig且不為空就取代
                }

                #endregion txtlog 檔案名稱

                #region save Treasury_Securities_Info(A42)

                MSGReturnModel resultA42 = A4Repository.saveA42(dataModel); //save to DB

                bool A42Log = CommonFunction.saveLog(Table_Type.A42,
                                                      fileName, SetFile.ProgramName, resultA42.RETURN_FLAG,
                                                      Debt_Type.B.ToString(), startTime, DateTime.Now); //寫sql Log
                TxtLog.txtLog(Table_Type.A42, resultA42.RETURN_FLAG, startTime, txtLocation(txtpath)); //寫txt Log

                #endregion save Treasury_Securities_Info(A42)

                result.RETURN_FLAG = resultA42.RETURN_FLAG;
                result.DESCRIPTION = Message_Type.save_Success
                    .GetDescription(Table_Type.A42.ToString());

                if (!result.RETURN_FLAG)
                {
                    result.DESCRIPTION = Message_Type.save_Fail
                        .GetDescription(Table_Type.A42.ToString(), resultA42.DESCRIPTION);
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
        /// 前端轉檔一系列動作
        /// </summary>
        /// <param name="type">目前要轉檔的表名</param>
        /// <param name="date">日期</param>
        /// <param name="version">版本(房貸沒有)</param>
        /// <param name="next">是否要執行下一項</param>
        /// <param name="debt">M:房貸 B:債券</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult TransferToOther(string type, string date, string version, bool next, string debt)
        {
            MSGReturnModel result = new MSGReturnModel();

            result.RETURN_FLAG = false;
            result.DESCRIPTION = Message_Type.parameter_Error.GetDescription();

            DateTime dat = DateTime.MinValue;
            int ver = 0;

            if ((Debt_Type.M.ToString().Equals(debt) ? false : version.IsNullOrWhiteSpace() || 
                !Int32.TryParse(version,out ver) || ver == 0) //房貸沒有版本 , 債券version不等於0
                || !DateTime.TryParse(date, out dat))
                return Json(result);

            string tableName = string.Empty;
            string fileName = Excel_UploadName.A41.GetDescription().GetExelName(); //預設
            if (Debt_Type.B.ToString().Equals(debt))
            {
                string configFileName = ConfigurationManager.AppSettings["fileA4Name"];
                if (!string.IsNullOrWhiteSpace(configFileName))
                    fileName = configFileName; //config 設定就取代
            }
            if (Debt_Type.M.ToString().Equals(debt))
                fileName = "A01-IAS39,A02";

            DateTime startTime = DateTime.Now;
            switch (type)
            {
                case "All": //All 也是重B01開始 B01 => C01
                case "B01":
                    result = A4Repository.saveB01(ver, dat, debt);
                    bool B01Log = CommonFunction.saveLog(Table_Type.B01, fileName, SetFile.ProgramName,
                        result.RETURN_FLAG, debt, startTime, DateTime.Now); //寫sql Log
                    result.Datas = Json(transferMessage(next, Transfer_Table_Type.C01.ToString())); //回傳要不要做下一個transfer
                    break;

                case "C01":
                    result = A4Repository.saveC01(ver, dat, debt);
                    bool C01Log = CommonFunction.saveLog(Table_Type.C01, fileName, SetFile.ProgramName,
                        result.RETURN_FLAG, debt, startTime, DateTime.Now); //寫sql Log
                    //債券最多到C01
                    result.Datas = Json(transferMessage((debt.Equals("B") ? false : next), Transfer_Table_Type.C02.ToString()));
                    break;

                case "C02":
                    result = A4Repository.saveC02(ver, dat, debt);
                    bool C02Log = CommonFunction.saveLog(Table_Type.C02, fileName, SetFile.ProgramName,
                        result.RETURN_FLAG, debt, startTime, DateTime.Now); //寫sql Log
                    result.Datas = Json(transferMessage(false, string.Empty)); //目前到C02 而已
                    break;
            }

            return Json(result);
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
                    result.DESCRIPTION = Message_Type.upload_Not_Find.GetDescription();
                    return Json(result);
                }

                #endregion 前端無傳送檔案進來

                #region 前端檔案大小不符或不為Excel檔案(驗證)

                //ModelState
                if (!ModelState.IsValid)
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = Message_Type.excel_Validate.GetDescription();
                    return Json(result);
                }

                #endregion 前端檔案大小不符或不為Excel檔案(驗證)

                #region 上傳檔案

                string pathType = Path.GetExtension(FileModel.File.FileName)
                                       .Substring(1); //上傳的檔案類型

                var fileName = string.Format("{0}.{1}",
                    Excel_UploadName.A41.GetDescription(),
                    pathType); //固定轉成此名稱

                Cache.Invalidate(CacheList.A41ExcelName); //清除 Cache
                Cache.Set(CacheList.A41ExcelName, fileName, 15); //把資料存到 Cache

                #region 檢查是否有FileUploads資料夾,如果沒有就新增 並加入 excel 檔案

                string projectFile = Server.MapPath("~/" + SetFile.FileUploads); //專案資料夾
                string path = Path.Combine(projectFile, fileName);

                FileRelated.createFile(projectFile); //檢查是否有FileUploads資料夾,如果沒有就新增

                //呼叫上傳檔案 function
                result = FileRelated.FileUpLoadinPath(path, FileModel.File);
                if (!result.RETURN_FLAG)
                    return Json(result);

                #endregion 檢查是否有FileUploads資料夾,如果沒有就新增 並加入 excel 檔案

                #region 讀取Excel資料 使用ExcelDataReader 並且組成 json

                var stream = FileModel.File.InputStream;
                List<A41ViewModel> dataModel = A4Repository.getExcel(pathType, stream);
                if (dataModel.Count > 0)
                {
                    result.RETURN_FLAG = true;
                    Cache.Invalidate(CacheList.A41ExcelfileData); //清除 Cache
                    Cache.Set(CacheList.A41ExcelfileData, dataModel, 15); //把資料存到 Cache
                }
                else
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = Message_Type.data_Not_Compare.GetDescription();
                }

                #endregion 讀取Excel資料 使用ExcelDataReader 並且組成 json

                #endregion 上傳檔案
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = ex.Message;
            }
            return Json(result);
        }

        /// <summary>
        /// 選擇檔案後點選資料上傳觸發
        /// </summary>
        /// <returns>MSGReturnModel</returns>
        [HttpPost]
        public JsonResult UploadA42()
        {
            MSGReturnModel result = new MSGReturnModel();

            //## 如果有任何檔案類型才做
            if (Request.Files.AllKeys.Any())
            {
                var FileModel = Request.Files["UploadedFile"];
                string processingDate = Request.Form["processingDate"];
                string reportDate = Request.Form["reportDate"];

                try
                {
                    #region 前端無傳送檔案進來

                    if (FileModel == null)
                    {
                        result.RETURN_FLAG = false;
                        result.DESCRIPTION = Message_Type.upload_Not_Find.GetDescription();
                        return Json(result);
                    }

                    #endregion 前端無傳送檔案進來

                    #region 前端檔案大小不符或不為Excel檔案(驗證)

                    //ModelState
                    if (!ModelState.IsValid)
                    {
                        result.RETURN_FLAG = false;
                        result.DESCRIPTION = Message_Type.excel_Validate.GetDescription();
                        return Json(result);
                    }
                    else
                    {
                        string ExtensionName = Path.GetExtension(FileModel.FileName).ToLower();
                        if (ExtensionName != ".xls" && ExtensionName != ".xlsx")
                        {
                            result.RETURN_FLAG = false;
                            result.DESCRIPTION = Message_Type.excel_Validate.GetDescription();
                            return Json(result);
                        }
                    }

                    #endregion 前端檔案大小不符或不為Excel檔案(驗證)

                    #region 上傳檔案

                    string pathType = Path.GetExtension(FileModel.FileName)
                       .Substring(1); //上傳的檔案類型

                    var fileName = string.Format("{0}.{1}",
                        Excel_UploadName.A42.GetDescription(),
                        pathType); //固定轉成此名稱

                    Cache.Invalidate(CacheList.A42ExcelName); //清除 Cache
                    Cache.Set(CacheList.A42ExcelName, fileName, 15); //把資料存到 Cache

                    #region 檢查是否有FileUploads資料夾,如果沒有就新增 並加入 excel 檔案

                    string projectFile = Server.MapPath("~/" + SetFile.FileUploads); //專案資料夾
                    string path = Path.Combine(projectFile, fileName);

                    FileRelated.createFile(projectFile); //檢查是否有FileUploads資料夾,如果沒有就新增

                    //呼叫上傳檔案 function
                    result = FileRelated.FileUpLoadinPath(path, FileModel);
                    if (!result.RETURN_FLAG)
                    {
                        return Json(result);
                    }

                    #endregion 檢查是否有FileUploads資料夾,如果沒有就新增 並加入 excel 檔案

                    #region 讀取Excel資料 使用ExcelDataReader 並且組成 json

                    var stream = FileModel.InputStream;
                    List<A42ViewModel> dataModel = A4Repository.getA42Excel(pathType, stream, processingDate, reportDate);
                    if (dataModel.Count > 0)
                    {
                        result.RETURN_FLAG = true;
                        result.Datas = Json(dataModel); //給JqGrid 顯示
                    }
                    else
                    {
                        result.RETURN_FLAG = false;
                        result.DESCRIPTION = Message_Type.data_Not_Compare.GetDescription();
                    }

                    #endregion 讀取Excel資料 使用ExcelDataReader 並且組成 json

                    #endregion 上傳檔案
                }
                catch (Exception ex)
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = ex.Message;
                }
            }
            else
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type.upload_Not_Find.GetDescription();
                return Json(result);
            }

            return Json(result);
        }

        /// <summary>
        /// 判斷轉檔有沒有後續
        /// </summary>
        /// <param name="next"></param>
        /// <param name="nextType"></param>
        /// <returns></returns>
        private string transferMessage(bool next, string nextType)
        {
            return next ? "true," + nextType : "false";
        }
    }
}