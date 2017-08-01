using System;
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
    public class C4Controller : CommonController
    {
        private IC4Repository C4Repository;
        private ICommon CommonFunction;
        public ICacheProvider Cache { get; set; }

        public C4Controller()
        {
            this.C4Repository = new C4Repository();
            this.CommonFunction = new Common();
            this.Cache = new DefaultCacheProvider();
        }

        /// <summary>
        /// A4(上傳檔案)
        /// </summary>
        /// <returns></returns>
        [UserAuth("Index,C4")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// A41(債券明細檔)
        /// </summary>
        /// <returns></returns>
        //[UserAuth("C41Detail,C4")]
        //public ActionResult C41Detail()
        //{
        //    return View();
        //}

        /// <summary>
        /// 選擇檔案後點選資料上傳觸發
        /// </summary>
        /// <param name="FileModel"></param>
        /// <returns>MSGReturnModel</returns>
        //[HttpPost]
        //public JsonResult Upload(ValidateFiles FileModel)
        //{
        //    MSGReturnModel result = new MSGReturnModel();
        //    try
        //    {
        //        #region 前端無傳送檔案進來
        //        if (FileModel.File == null)
        //        {
        //            result.RETURN_FLAG = false;
        //            result.DESCRIPTION = Message_Type.upload_Not_Find.GetDescription();
        //            return Json(result);
        //        }
        //        #endregion

        //        #region 前端檔案大小不符或不為Excel檔案(驗證)
        //        //ModelState
        //        if (FileModel.File.ContentLength == 0 )
        //        {
        //            result.RETURN_FLAG = false;
        //            result.DESCRIPTION = Message_Type.excel_Validate.GetDescription();
        //            return Json(result);
        //        }
        //        #endregion

        //        #region 上傳檔案
        //        var fileName = Path.GetFileName(FileModel.File.FileName); //檔案名稱

        //        #region 檢查是否有FileUploads資料夾,如果沒有就新增 並加入 excel 檔案

        //        string projectFile = Server.MapPath("~/FileUploads"); //專案資料夾
        //        string path = Path.Combine(projectFile, fileName);

        //        FileRelated.createFile(projectFile); //檢查是否有FileUploads資料夾,如果沒有就新增

        //        //呼叫上傳檔案 function
        //        result = FileRelated.FileUpLoadinPath(path, FileModel.File);
        //        if (!result.RETURN_FLAG)
        //            return Json(result);
        //        #endregion

        //        #region 讀取Excel資料 使用ExcelDataReader 並且組成 json
        //        string pathType =
        //            Path.GetExtension(FileModel.File.FileName)
        //            .Substring(1); //檔案類型
        //        var stream = FileModel.File.InputStream;
        //        List<A41ViewModel> dataModel = A4Repository.getExcel(pathType, stream);
        //        if (dataModel.Count > 0)
        //        {
        //            result.RETURN_FLAG = true;
        //            Cache.Invalidate("A41ExcelfileData"); //清除 Cache
        //            Cache.Set("A41ExcelfileData", dataModel, 15); //把資料存到 Cache
        //        }
        //        else
        //        {
        //            result.RETURN_FLAG = false;
        //            result.DESCRIPTION = Message_Type.data_Not_Compare.GetDescription();
        //        }
        //        #endregion

        //        #endregion
        //    }
        //    catch (Exception ex)
        //    {
        //        result.RETURN_FLAG = false;
        //        result.DESCRIPTION = ex.Message;
        //    }
        //    return Json(result);
        //}

        [HttpPost]
        public JsonResult Transfer()
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {

                DateTime startTime = DateTime.Now;
                string projectFile = Server.MapPath("~/C4Uploads");
                FileRelated.createFile(projectFile);
                string fileName = @"Sample_20170726"; //預設
                string path = Path.Combine(projectFile, fileName);

                C4Repository.test(projectFile,fileName);
                result.RETURN_FLAG = true;
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