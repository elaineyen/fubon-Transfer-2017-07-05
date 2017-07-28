﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
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
    public class A4Controller : CommonController
    {
        private string[] selects = { "All", "B01","C01" };
        private IA4Repository A4Repository;
        private ICommon CommonFunction;
        private IFRS9Entities db = new IFRS9Entities();
        public ICacheProvider Cache { get; set; }

        public A4Controller()
        {
            this.A4Repository = new A4Repository();
            this.CommonFunction = new Common();
            this.Cache = new DefaultCacheProvider();
        }

        /// <summary>
        /// A4(上傳檔案)
        /// </summary>
        /// <returns></returns>
        [UserAuth("Index,A4")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// A41(債券明細檔)
        /// </summary>
        /// <returns></returns>
        [UserAuth("A41Detail,A4")]
        public ActionResult A41Detail()
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
                selects.Select(x => new { Text = x, Value = x }), "Value", "Text");
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

                #region 前端檔案大小不符或不為Excel檔案(驗證)
                //ModelState
                if (FileModel.File.ContentLength == 0 )
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
                List<A41ViewModel> dataModel = A4Repository.getExcel(pathType, stream);
                if (dataModel.Count > 0)
                {
                    result.RETURN_FLAG = true;
                    Cache.Invalidate("A41ExcelfileData"); //清除 Cache
                    Cache.Set("A41ExcelfileData", dataModel, 15); //把資料存到 Cache
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
                result.DESCRIPTION = ex.Message;
            }
            return Json(result);
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
                    if (Cache.IsSet("A41ExcelfileData"))
                        data = (List<A41ViewModel>)Cache.Get("A41ExcelfileData");  //從Cache 抓資料
                    break;
                case "Db":
                    if (Cache.IsSet("A41DbfileData"))
                        data = (List<A41ViewModel>)Cache.Get("A41DbfileData");
                    break;
            }
            //List<A41ViewModel> data =
            //A4Repository.tempA41(); //從Cache 抓資料
            //var result = Jqgrid.modelToJqgridResult(jdata, data);
            //return Json(result);
            return Json(jdata.modelToJqgridResult(data));
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
                string fileName = @"Data Requirements.xlsx"; //預設
                string configFileName = ConfigurationManager.AppSettings["fileA4Name"];
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
                string txtpath = "DataRequirementsTransfer.txt"; //預設txt名稱
                string configTxtName = ConfigurationManager.AppSettings["txtLogA4Name"];
                if (!string.IsNullOrWhiteSpace(configTxtName))
                    txtpath = configTxtName; //有設定webConfig且不為空就取代
                #endregion

                #region save Bond_Account_Info(A41)
                var table = Table_Type.A41;
                tableName = table.GetDescription() ;
                MSGReturnModel resultA41 = A4Repository.saveA41(dataModel); //save to DB
                bool A41Log = CommonFunction.saveLog(table.ToString(),tableName,
                    fileName, proName, resultA41.RETURN_FLAG,
                    Debt_Type.B.ToString(), startTime, DateTime.Now); //寫sql Log
                TxtLog.txtLog(tableName, resultA41.RETURN_FLAG, startTime, txtLocation(txtpath)); //寫txt Log
                #endregion

                result.RETURN_FLAG = resultA41.RETURN_FLAG;
                result.DESCRIPTION = Message_Type.save_Success.GetDescription("A41");

                if (!result.RETURN_FLAG)
                {
                    result.DESCRIPTION = Message_Type.save_Fail
                        .GetDescription("A41", resultA41.DESCRIPTION);
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
        public JsonResult GetData(string type,string searchType,string value,string date)
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
                            var A41Data = A4Repository.GetA41(searchType, value,d);
                            result.RETURN_FLAG = A41Data.Item1;
                            Cache.Invalidate("A41DbfileData"); //清除
                            Cache.Set("A41DbfileData", A41Data.Item2, 15); //把資料存到 Cache
                        //}
                        //else
                        //{
                        //    result.RETURN_FLAG = true; //有Cache資料
                        //}
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
        /// 前端轉檔一系列動作
        /// </summary>
        /// <param name="type">目前要轉檔的表名</param>
        /// <param name="date">日期</param>
        /// <param name="version">版本(房貸沒有)</param>
        /// <param name="next">是否要執行下一項</param>
        /// <param name="debt">M:房貸 B:債券</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult TransferToOther(string type, string date, string version, bool next,string debt)
        {
            MSGReturnModel result = new MSGReturnModel();

            result.RETURN_FLAG = false;
            result.DESCRIPTION = "傳入參數錯誤!";

            DateTime dat = DateTime.MinValue;

            if ((Debt_Type.M.ToString().Equals(debt) ? false : version.IsNullOrWhiteSpace()) //房貸沒有版本
                || !DateTime.TryParse(date, out dat))
                return Json(result);

            string tableName = string.Empty;
            string fileName = @"Data Requirements.xlsx"; //預設
            if (Debt_Type.B.ToString().Equals(debt))
            {
                string configFileName = ConfigurationManager.AppSettings["fileA4Name"];
                if (!string.IsNullOrWhiteSpace(configFileName))
                    fileName = configFileName; //config 設定就取代
            }
            if (Debt_Type.M.ToString().Equals(debt))
                fileName = "A01-IAS39,A02";

            string proName = "Transfer";
            DateTime startTime = DateTime.Now;
            switch (type)
            {
                case "All": //All 也是重B01開始 B01 => C01
                case "B01":
                    result = A4Repository.saveB01(version, dat, debt);
                    tableName = Table_Type.B01.GetDescription();
                    bool B01Log = CommonFunction.saveLog("B01", tableName, fileName, proName, 
                        result.RETURN_FLAG, debt, startTime, DateTime.Now); //寫sql Log
                    result.Datas = Json(transferMessage(next, "C01")); //回傳要不要做下一個transfer
                    break;
                case "C01":
                    result = A4Repository.saveC01(version, dat, debt);
                    tableName = Table_Type.C01.GetDescription();
                    bool C01Log = CommonFunction.saveLog("C01", tableName, fileName, proName, 
                        result.RETURN_FLAG, debt, startTime, DateTime.Now); //寫sql Log
                    result.Datas = Json(transferMessage(false, string.Empty)); //目前到C01 而已
                    break;
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
            List<string> logDatas = A4Repository.GetLogData(selects.ToList(), debt);
            return Json(string.Join(",",logDatas));
        }

        /// <summary>
        /// 判斷轉檔有沒有後續 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="nextType"></param>
        /// <returns></returns>
        private string transferMessage(bool next,string nextType )
        {
            return next ? "true," + nextType : "false";
        }

    }
}