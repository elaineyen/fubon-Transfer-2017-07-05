using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web.Mvc;
using Transfer.Models.Interface;
using Transfer.Models.Repositiry;
using Transfer.Utility;
using Transfer.ViewModels;

namespace Transfer.Controllers
{
    public class A7Controller : CommonController
    {
        private IA7Repository A7Repository;
        private ICommon CommonFunction;

        public A7Controller()
        {
            this.A7Repository = new A7Repository();
            this.CommonFunction = new Common();
        }

        // GET: A7
        public ActionResult Index()
        {
            ViewBag.Manu = "A7Main";
            ViewBag.SubManu = "A70SubMain";
            return View();
        }

        public ActionResult A71Detail()
        {
            ViewBag.Manu = "A7Main";
            ViewBag.SubManu = "A71SubMain";
            return View();
        }

        public ActionResult A72Detail()
        {
            ViewBag.Manu = "A7Main";
            ViewBag.SubManu = "A72SubMain";
            return View();
        }

        public ActionResult A73Detail()
        {
            ViewBag.Manu = "A7Main";
            ViewBag.SubManu = "A73SubMain";
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

                #region save Moody_Tm_YYYY(A71)
                tableName = "Moody_Tm_YYYY";
                bool flagA7 = A7Repository.saveA7(dataModel); //save to DB
                bool A71Log = CommonFunction.saveLog(tableName, fileName, proName, flagA7, startTime, DateTime.Now); //寫sql Log
                TxtLog.txtLog(tableName, flagA7, startTime, txtLocation(txtpath)); //寫txt Log
                #endregion

                result.RETURN_FLAG = flagA7;
                if (!result.RETURN_FLAG)
                {
                    result.DESCRIPTION = "Save Error!";
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

        /// <summary>
        /// 下載 Excel (A72,A73)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetExcel(string type)
        {
            MSGReturnModel result = new MSGReturnModel();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = "請確認檔案是否開啟!";
            string path = string.Empty;
            try
            {
                switch (type)
                {
                    case "A72":
                        path = @"A72.xlsx"; //預設
                        result = A7Repository.DownLoadExcel(type, ExcelLocation(path));                     
                        break;
                    case "A73":
                        path = @"A73.xlsx"; //預設
                        result = A7Repository.DownLoadExcel(type, ExcelLocation(path));
                        break;
                }
                if (result.RETURN_FLAG)
                    result.DESCRIPTION = "Success !";
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = ex.Message;
            }
            return Json(result);
        }
    }
}