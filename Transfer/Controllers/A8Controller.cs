using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web.Mvc;
using Transfer.Infrastructure;
using Transfer.Models;
using Transfer.Models.Interface;
using Transfer.Models.Repositiry;
using Transfer.Utility;
using Transfer.ViewModels;
using static Transfer.Enum.Ref;

namespace Transfer.Controllers
{
    [Authorize]
    public class A8Controller : CommonController
    {
        private IA8Repository A8Repository;
        private ICommon CommonFunction;
       
        public A8Controller()
        {
            this.A8Repository = new A8Repository();
            this.CommonFunction = new Common();
        }

        /// <summary>
        /// A8(上傳檔案)
        /// </summary>
        /// <returns></returns>
        [UserAuth("Index,A8")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// A8(查詢(A81.A82.A83))
        /// </summary>
        /// <returns></returns>
        [UserAuth("Detail,A8")]
        public ActionResult Detail()
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
                List<Exhibit10Model> dataModel = A8Repository.getExcel(pathType, stream);
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
                result.DESCRIPTION = Message_Type.upload_Fail.GetDescription(null, ex.Message);
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
                List<Exhibit10Model> dataModel = A8Repository.getExcel(pathType, stream); //Excel轉成 Exhibit10Model

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
                tableName = Table_Type.A81.GetDescription();
                MSGReturnModel resultA81 = A8Repository.SaveA8("A81",dataModel); //save to DB
                bool A81Log = CommonFunction.saveLog("A81",tableName, fileName, proName, 
                    resultA81.RETURN_FLAG, Debt_Type.B.ToString(), startTime, DateTime.Now); //寫sql Log
                TxtLog.txtLog(tableName, resultA81.RETURN_FLAG, startTime, txtLocation(txtpath)); //寫txt Log
                #endregion

                #region save Moody_Quartly_PD_Info(A82)
                tableName = Table_Type.A82.GetDescription();
                MSGReturnModel resultA82 = A8Repository.SaveA8("A82",dataModel); //save to DB
                bool A82Log = CommonFunction.saveLog("A82",tableName, fileName, proName,
                    resultA82.RETURN_FLAG, Debt_Type.B.ToString(), startTime, DateTime.Now); //寫sql Log
                TxtLog.txtLog(tableName, resultA82.RETURN_FLAG, startTime, txtLocation(txtpath)); //寫txt Log
                #endregion

                #region save Moody_Predit_PD_Info(A83)
                tableName = Table_Type.A83.GetDescription();
                MSGReturnModel resultA83 = A8Repository.SaveA8("A83",dataModel); //save to DB
                bool A83Log = CommonFunction.saveLog("A83",tableName, fileName, proName,
                    resultA83.RETURN_FLAG, Debt_Type.B.ToString(), startTime, DateTime.Now); //寫sql Log
                TxtLog.txtLog(tableName, resultA83.RETURN_FLAG, startTime, txtLocation(txtpath)); //寫txt Log
                #endregion

                result.RETURN_FLAG = resultA81.RETURN_FLAG &&
                                     resultA82.RETURN_FLAG &&
                                     resultA83.RETURN_FLAG;
                result.DESCRIPTION = Message_Type.save_Success.GetDescription("A81,A82,A83");

                if (!result.RETURN_FLAG)
                {
                    List<string> errs = new List<string>();
                    if (!resultA81.RETURN_FLAG)
                        errs.Add(resultA81.DESCRIPTION);
                    if (!resultA82.RETURN_FLAG)
                        errs.Add(resultA82.DESCRIPTION);
                    if (!resultA83.RETURN_FLAG)
                        errs.Add(resultA83.DESCRIPTION);

                    result.DESCRIPTION = string.Join("\n", errs);
                }

            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type.save_Fail.GetDescription(null,ex.Message);
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
            result.DESCRIPTION = Message_Type.not_Find_Any.GetDescription(type);
            try
            {
                switch (type)
                {
                    case "A81": //抓Moody_Monthly_PD_Info(A81)資料
                        var A81Data = A8Repository.GetA81();
                        result.RETURN_FLAG = A81Data.Item1;
                        result.Datas = Json(A81Data.Item2);
                        break;
                    case "A82"://抓Moody_Quartly_PD_Info(A82)資料
                        var A82Data = A8Repository.GetA82();
                        result.RETURN_FLAG = A82Data.Item1;
                        result.Datas = Json(A82Data.Item2);
                        break;
                    case "A83"://抓Moody_Predit_PD_Info(A83)資料
                        var A83Data = A8Repository.GetA83();
                        result.RETURN_FLAG = A83Data.Item1;
                        result.Datas = Json(A83Data.Item2);
                        break;
                }
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type.not_Find_Any.GetDescription(type,ex.Message);
            }
            return Json(result);
        }

    }
}