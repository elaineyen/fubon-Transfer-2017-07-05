using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web.Mvc;
using Transfer.Infrastructure;
using Transfer.Models.Interface;
using Transfer.Models.Repositiry;
using Transfer.Utility;
using Transfer.ViewModels;
using static Transfer.Enum.Ref;

namespace Transfer.Controllers
{
    [Authorize]
    public class A7Controller : CommonController
    {
        private IA7Repository A7Repository;
        private ICommon CommonFunction;

        public A7Controller()
        {
            this.A7Repository = new A7Repository();
            this.CommonFunction = new Common();
        }

        /// <summary>
        /// A7 (上傳檔案)
        /// </summary>
        /// <returns></returns>
        [UserAuth("Index,A7")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 查詢A71 (轉移矩陣資料檔_Moody)
        /// </summary>
        /// <returns></returns>
        [UserAuth("A71Detail,A7")]
        public ActionResult A71Detail()
        {
            return View();
        }

        /// <summary>
        /// 查詢A72 (轉移矩陣資料檔_調整後)
        /// </summary>
        /// <returns></returns>
        [UserAuth("A72Detail,A7")]
        public ActionResult A72Detail()
        {
            return View();
        }

        /// <summary>
        /// 查詢A73 (等級違約率矩陣)
        /// </summary>
        /// <returns></returns>
        [UserAuth("A73Detail,A7")]
        public ActionResult A73Detail()
        {
            return View();
        }

        /// <summary>
        /// 查詢A51 (信評主標尺對應檔)
        /// </summary>
        /// <returns></returns>
        [UserAuth("A74Detail,A7")]
        public ActionResult A74Detail()
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
                    result.DESCRIPTION = Message_Type.upload_Not_Find.GetDescription();
                    return Json(result);
                }
                #endregion

                #region 前端檔案大小不服或不為Excel檔案(驗證)
                if (FileModel.File.ContentLength == 0 || !ModelState.IsValid)
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = Message_Type.excel_Validate.GetDescription();
                    return Json(result);
                }
                #endregion

                #region 上傳檔案
                var fileName = Path.GetFileName(FileModel.File.FileName); //檔案名稱

                #region 檢查是否有FileUploads資料夾,如果沒有就新增 並加入 excel 檔案

                string projectFile = Server.MapPath("~/FileUploads"); //專案資料夾
                string path = Path.Combine(projectFile, fileName);
                FileRelated.createFile(projectFile); //檢查是否有FileUploads資料夾,如果沒有就新增

                //呼叫上傳檔案 function
                result = FileRelated.FileUpLoadinPath(path, FileModel.File);
                if (!result.RETURN_FLAG)
                    return Json(result);
                #endregion

                #region 讀取Excel資料 使用ExcelDataReader 並且組成 json
                string pathType =
                    Path.GetExtension(FileModel.File.FileName)
                    .Substring(1); //檔案類型
                var stream = FileModel.File.InputStream;
                List<Exhibit29Model> dataModel = A7Repository.getExcel(pathType, stream);
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
                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type.upload_Fail
                    .GetDescription(FileModel.File.FileName, ex.Message);
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
                string fileName = @"Exhibit 29.xlsx"; //預設
                string configFileName = ConfigurationManager.AppSettings["fileA7Name"];
                if (!string.IsNullOrWhiteSpace(configFileName))
                    fileName = configFileName; //config 設定就取代
                string path = Path.Combine(projectFile, fileName);
                FileStream stream = System.IO.File.Open(path, FileMode.Open, FileAccess.Read);

                string pathType = path.Split('.')[1]; //抓副檔名
                List<Exhibit29Model> dataModel = A7Repository.getExcel(pathType, stream); //Excel轉成 Exhibit10Model

                string proName = "Transfer";
                string tableName = string.Empty;
                #endregion

                #region txtlog 檔案名稱
                string txtpath = "Exhibit29Transfer.txt"; //預設txt名稱
                string configTxtName = ConfigurationManager.AppSettings["txtLogA7Name"];
                if (!string.IsNullOrWhiteSpace(configTxtName))
                    txtpath = configTxtName; //有設定webConfig且不為空就取代
                #endregion

                #region save 資料
                #region save Moody_Tm_YYYY(A71)
                tableName = Table_Type.A71.GetDescription();
                MSGReturnModel resultA71 = A7Repository.saveA71(dataModel); //save to DB
                bool A71Log = CommonFunction.saveLog("A71",tableName, fileName, proName,
                    resultA71.RETURN_FLAG,Debt_Type.B.ToString(), startTime, DateTime.Now); //寫sql Log
                TxtLog.txtLog(tableName, resultA71.RETURN_FLAG, startTime, txtLocation(txtpath)); //寫txt Log
                #endregion
                #region save Tm_Adjust_YYYY(A72)
                tableName = Table_Type.A72.GetDescription();
                MSGReturnModel resultA72 = A7Repository.saveA72(); //save to DB
                bool A72Log = CommonFunction.saveLog("A72",tableName, fileName, proName, 
                    resultA72.RETURN_FLAG, Debt_Type.B.ToString(), startTime, DateTime.Now); //寫sql Log
                TxtLog.txtLog(tableName, resultA72.RETURN_FLAG, startTime, txtLocation(txtpath)); //寫txt Log
                #endregion
                #region save GM_YYYY(A73)
                tableName = Table_Type.A73.GetDescription();
                MSGReturnModel resultA73 = A7Repository.saveA73(); //save to DB
                bool A73Log = CommonFunction.saveLog("A73",tableName, fileName, proName, 
                    resultA73.RETURN_FLAG, Debt_Type.B.ToString(), startTime, DateTime.Now); //寫sql Log
                TxtLog.txtLog(tableName, resultA73.RETURN_FLAG, startTime, txtLocation(txtpath)); //寫txt Log
                #endregion
                #region save Grade_Moody_Info(A51)
                tableName = Table_Type.A51.GetDescription();
                MSGReturnModel resultA51 = A7Repository.saveA51(); //save to DB
                bool A51Log = CommonFunction.saveLog("A51",tableName, fileName, proName,
                    resultA51.RETURN_FLAG, Debt_Type.B.ToString(), startTime, DateTime.Now); //寫sql Log
                TxtLog.txtLog(tableName, resultA51.RETURN_FLAG, startTime, txtLocation(txtpath)); //寫txt Log
                #endregion

                result.RETURN_FLAG = resultA71.RETURN_FLAG &&
                                     resultA72.RETURN_FLAG &&
                                     resultA73.RETURN_FLAG &&
                                     resultA51.RETURN_FLAG;

                result.DESCRIPTION = Message_Type.save_Success.GetDescription("A71,A72,A73,A51");

                if (!result.RETURN_FLAG)
                {
                    List<string> errs = new List<string>();
                    if (!resultA71.RETURN_FLAG)
                        errs.Add(resultA71.DESCRIPTION);
                    if (!resultA72.RETURN_FLAG)
                        errs.Add(resultA72.DESCRIPTION);
                    if (!resultA73.RETURN_FLAG)
                        errs.Add(resultA73.DESCRIPTION);
                    if (!resultA51.RETURN_FLAG)
                        errs.Add(resultA51.DESCRIPTION);

                    result.DESCRIPTION = Message_Type.save_Fail
                        .GetDescription(null, string.Join("\n", errs));
                }
                #endregion
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type.save_Fail
                    .GetDescription(null, ex.Message);
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
            try
            {
                switch (type)
                {
                    case "A71": //Moody_Tm_YYYY(A71)資料
                        var A71 = A7Repository.GetA71();
                        result.RETURN_FLAG = A71.Item1;
                        result.Datas = Json(A71.Item2);
                        break;
                    case "A72"://抓Tm_Adjust_YYYY(A72)資料
                        var A72 = A7Repository.GetA72();
                        result.RETURN_FLAG = A72.Item1;
                        result.Datas = Json(A72.Item2);
                        break;
                    case "A73"://抓GM_YYYY(A73)資料
                        var A73 = A7Repository.GetA73();
                        result.RETURN_FLAG = A73.Item1;
                        result.Datas = Json(A73.Item2);
                        break;
                    case "A51"://抓Grade_Moody_Info(A51)資料
                        var A51 = A7Repository.GetA51();
                        result.RETURN_FLAG = A51.Item1;
                        result.Datas = Json(A51.Item2);
                        break;
                }
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type.not_Find_Any.
                    GetDescription(type,ex.Message);
            }
            return Json(result);
        }

        /// <summary>
        /// 下載 Excel (A72,A73)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetExcel(string type)
        {
            MSGReturnModel result = new MSGReturnModel();
            string path = string.Empty;
            try
            {
                switch (type)
                {
                    case "A72":
                        //path = @"A72.xlsx"; //預設 (2007)
                        path = @"A72.xls"; //預設 (2003)
                        result = A7Repository.DownLoadExcel(type, ExcelLocation(path));
                        break;
                    case "A73":
                        //path = @"A73.xlsx"; //預設 (2007)
                        path = @"A73.xls"; //預設  (2003)
                        result = A7Repository.DownLoadExcel(type, ExcelLocation(path));
                        break;
                }
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type.download_Fail
                    .GetDescription(type, ex.Message);
            }
            return Json(result);
        }
    }
}