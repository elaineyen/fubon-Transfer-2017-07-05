﻿using System;
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
    public class A6Controller : CommonController
    {
        private IA6Repository A6Repository;
        private ICommon CommonFunction;

        public A6Controller()
        {
            this.A6Repository = new A6Repository();
            this.CommonFunction = new Common();
            this.Cache = new DefaultCacheProvider();
        }

        public ICacheProvider Cache { get; set; }

        /// <summary>
        /// 查詢A62 (違約損失資料檔_歷史資料 Moody_LGD_Info)
        /// </summary>
        /// <returns></returns>
        [UserAuth("A62Detail,A6")]
        public ActionResult A62Detail()
        {
            ViewBag.year = new SelectList(
                A6Repository.GetA62SearchYear()
                .Select(x => new { Text = x, Value = x }), "Value", "Text");
            return View();
        }

        /// <summary>
        /// 前端抓資料時呼叫
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetData(string type, string year)
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                switch (type)
                {
                    case "A62": //Moody_LGD_Info(A62)資料
                        var A62 = A6Repository.GetA62(year);
                        result.RETURN_FLAG = A62.Item1;
                        var jqgridParams = new Exhibit7Model().TojqGridData();
                        jqgridParams.Datas = A62.Item2;
                        result.Datas = Json(jqgridParams);
                        break;
                }
                if (!result.RETURN_FLAG)
                    result.DESCRIPTION = Message_Type.not_Find_Any.GetDescription(type);
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type.not_Find_Any.
                    GetDescription(type, ex.Message);
            }
            return Json(result);
        }

        /// <summary>
        /// A6 (上傳檔案)
        /// </summary>
        /// <returns></returns>
        [UserAuth("Index,A6")]
        public ActionResult Index()
        {
            return View();
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
                string projectFile = Server.MapPath("~/" + SetFile.FileUploads);
                string fileName = string.Empty;
                if (Cache.IsSet(CacheList.A62ExcelName))
                    fileName = (string)Cache.Get(CacheList.A62ExcelName);  //從Cache 抓資料
                if (fileName.IsNullOrWhiteSpace())
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = Message_Type.time_Out.GetDescription();
                }

                string path = Path.Combine(projectFile, fileName);
                FileStream stream = System.IO.File.Open(path, FileMode.Open, FileAccess.Read);

                string pathType = path.Split('.')[1]; //抓副檔名
                List<Exhibit7Model> dataModel = A6Repository.getExcel(pathType, stream); //Excel轉成 Exhibit7Model

                #endregion 抓Excel檔案 轉成 model

                #region txtlog 檔案名稱

                string txtpath = SetFile.A62TransferTxtLog; //預設txt名稱
                string configTxtName = ConfigurationManager.AppSettings["txtLogA6Name"];
                if (!string.IsNullOrWhiteSpace(configTxtName))
                    txtpath = configTxtName; //有設定webConfig且不為空就取代

                #endregion txtlog 檔案名稱

                #region save 資料

                #region save Tm_Adjust_YYYY(A62)

                MSGReturnModel resultA62 = A6Repository.saveA62(dataModel); //save to DB
                bool A62Log = CommonFunction.saveLog(Table_Type.A62, fileName, SetFile.ProgramName,
                    resultA62.RETURN_FLAG, Debt_Type.B.ToString(), startTime, DateTime.Now); //寫sql Log
                TxtLog.txtLog(Table_Type.A62, resultA62.RETURN_FLAG, startTime, txtLocation(txtpath)); //寫txt Log

                #endregion save Tm_Adjust_YYYY(A62)

                result = resultA62;

                #endregion save 資料
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

                #region 前端檔案大小不服或不為Excel檔案(驗證)

                if (!ModelState.IsValid)
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = Message_Type.excel_Validate.GetDescription();
                    return Json(result);
                }

                #endregion 前端檔案大小不服或不為Excel檔案(驗證)

                #region 上傳檔案

                string pathType = Path.GetExtension(FileModel.File.FileName)
                                      .Substring(1); //上傳的檔案類型

                var fileName = string.Format("{0}.{1}",
                    Excel_UploadName.A62.GetDescription(),
                    pathType); //固定轉成此名稱

                Cache.Invalidate(CacheList.A62ExcelName); //清除 Cache
                Cache.Set(CacheList.A62ExcelName, fileName, 15); //把資料存到 Cache

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
                List<Exhibit7Model> dataModel = A6Repository.getExcel(pathType, stream);
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
                result.DESCRIPTION = Message_Type.upload_Fail
                    .GetDescription(FileModel.File.FileName, ex.Message);
            }
            return Json(result);
        }
    }
}