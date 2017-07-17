using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web.Mvc;
using Transfer.Models;
using Transfer.Models.Interface;
using Transfer.Models.Repositiry;
using Transfer.Utility;
using Transfer.ViewModels;

namespace Transfer.Controllers
{
    [Authorize]
    public class A4Controller : CommonController
    {
        private IA4Repository A4Repository;
        private ICommon CommonFunction;

        private IFRS9Entities db = new IFRS9Entities();

        public A4Controller()
        {
            this.A4Repository = new A4Repository();
            this.CommonFunction = new Common();
        }

        /// <summary>
        /// A4(上傳檔案)
        /// </summary>
        /// <returns></returns>
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
                List<A41ViewModel> dataModel = new List<A41ViewModel>();
                    //A8Repository.getExcel(pathType, stream);
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
                List<A41ViewModel> dataModel = A4Repository.getExcel(pathType, stream); //Excel轉成 Exhibit10Model

                string proName = "Transfer";
                string tableName = string.Empty;
                #endregion

                #region txtlog 檔案名稱
                string txtpath = "Exhibit10Transfer.txt"; //預設txt名稱
                string configTxtName = ConfigurationManager.AppSettings["txtLogA8Name"];
                if (!string.IsNullOrWhiteSpace(configTxtName))
                    txtpath = configTxtName; //有設定webConfig且不為空就取代
                #endregion

                #region save Moody_Monthly_PD_Info(A81)
                tableName = "Moody_Monthly_PD_Info";
                MSGReturnModel resultA41 = new MSGReturnModel();
                    //A4Repository.SaveA8("A81",dataModel); //save to DB
                bool A41Log = CommonFunction.saveLog(tableName, fileName, proName, resultA41.RETURN_FLAG, startTime, DateTime.Now); //寫sql Log
                TxtLog.txtLog(tableName, resultA41.RETURN_FLAG, startTime, txtLocation(txtpath)); //寫txt Log
                #endregion


                result.RETURN_FLAG = resultA41.RETURN_FLAG;
                result.DESCRIPTION = "Success!";

                if (!result.RETURN_FLAG)
                {
                    List<string> errs = new List<string>();
                    if (!resultA41.RETURN_FLAG)
                        errs.Add("SaveA41 Error: " + resultA41.DESCRIPTION);

                    result.DESCRIPTION = string.Join("\n", errs);
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
                    case "A41": //抓Moody_Monthly_PD_Info(A81)資料
                        //var A81Data = A8Repository.GetA81();
                        //result.RETURN_FLAG = A81Data.Item1;
                        //result.Datas = Json(A81Data.Item2);
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

    }
}