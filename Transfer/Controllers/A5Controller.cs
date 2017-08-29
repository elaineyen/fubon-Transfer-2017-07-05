using System;
using System.Collections.Generic;
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
    public class A5Controller : CommonController
    {
        private ICommon CommonFunction;
        private IA5Repository A5Repository;
        private List<SelectOption> searchOption = null;
        private List<SelectOption> sType = null;
        private List<SelectOption> actions = null;

        public A5Controller()
        {
            this.A5Repository = new A5Repository();
            this.CommonFunction = new Common();
            this.Cache = new DefaultCacheProvider();
            searchOption = new List<SelectOption>();
            searchOption.AddRange(EnumUtil.GetValues<A59_SelectType>()
                        .Select(x => new SelectOption()
                        {
                            text = x.GetDescription(),
                            value = x.ToString()
                        }));
            sType = new List<SelectOption>() {
                new SelectOption() {text=Rating_Type.A.GetDescription(),value="1" },
                new SelectOption() {text=Rating_Type.B.GetDescription(),value="2" }};
            actions = new List<SelectOption>() {
                new SelectOption() {text="查詢&下載",value="downLoad" },
                new SelectOption() {text="上傳&存檔",value="upLoad" }};
        }

        public ICacheProvider Cache { get; set; }

        /// <summary>
        /// 債券信評補登(歷史檔)
        /// </summary>
        /// <returns></returns>
        [UserAuth("A57Detail,A5")]
        public ActionResult A57Detail()
        {
            ViewBag.searchOption = new SelectList(searchOption, "Value", "Text");
            ViewBag.sType = new SelectList(sType, "Value", "Text");
            return View();
        }

        /// <summary>
        /// 債券信評補登(整理檔)
        /// </summary>
        /// <returns></returns>
        [UserAuth("A58Detail,A5")]
        public ActionResult A58Detail()
        {
            ViewBag.searchOption = new SelectList(searchOption, "Value", "Text");
            ViewBag.sType = new SelectList(sType, "Value", "Text");
            ViewBag.action = new SelectList(actions, "Value", "Text");
            return View();
        }

        /// <summary>
        /// 查詢A58資料
        /// </summary>
        /// <param name="datepicker">報導日</param>
        /// <param name="sType">評等種類(1:原始投資信評 ,2:評估日最近信評)</param>
        /// <param name="from">購入日期(起始)</param>
        /// <param name="to">購入日期(結束)</param>
        /// <param name="bondNumber">債券編號</param>
        /// <param name="version">資料版本</param>
        /// <param name="search">All:全查, Miss:查缺Grade_Adjust</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SearchA58(
            string datepicker,
            string sType,
            string from,
            string to,
            string bondNumber,
            string version,
            string search)
        {
            MSGReturnModel result = new MSGReturnModel();
            result.RETURN_FLAG = false;
            try
            {
                if (search.IsNullOrWhiteSpace())
                {
                    result.DESCRIPTION = Message_Type.parameter_Error.GetDescription();
                    return Json(result);
                }
                var A58Data = A5Repository.GetA58(datepicker, sType, from, to, bondNumber, version, search);
                result.RETURN_FLAG = A58Data.Item1;
                if (A58Data.Item1)
                {
                    if (search.IndexOf("Miss") > -1)
                    {
                        Cache.Invalidate(CacheList.A58DbMissfileData); //清除
                        Cache.Set(CacheList.A58DbMissfileData, A58Data.Item2, 15); //把資料存到 Cache
                    }
                    else
                    {
                        Cache.Invalidate(CacheList.A58DbfileData); //清除
                        Cache.Set(CacheList.A58DbfileData, A58Data.Item2, 15); //把資料存到 Cache
                    }
                }
                else
                {
                    result.DESCRIPTION = Message_Type.not_Find_Any.GetDescription();
                }
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = ex.Message;
            }
            return Json(result);
        }

        /// <summary>
        /// Get Cache Data
        /// </summary>
        /// <param name="jdata"></param>
        /// <param name="action">downLoad or upLoad</param>
        /// <param name="type">downLoad(All:全查 or Miss:查缺Grade_Adjust)</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata, string action, string type)
        {
            List<A58ViewModel> A58Data = new List<A58ViewModel>();
            List<A59ViewModel> A59Data = new List<A59ViewModel>();
            switch (type)
            {
                case "Miss":
                    if (Cache.IsSet(CacheList.A58DbMissfileData))
                        A58Data = (List<A58ViewModel>)Cache.Get(CacheList.A58DbMissfileData);  //從Cache 抓資料
                    break;

                case "All":
                    if (Cache.IsSet(CacheList.A58DbfileData))
                        A58Data = (List<A58ViewModel>)Cache.Get(CacheList.A58DbfileData);
                    break;

                case "A59":
                    if (Cache.IsSet(CacheList.A59ExcelfileData))
                        A59Data = (List<A59ViewModel>)Cache.Get(CacheList.A59ExcelfileData);
                    break;
            }
            if (action.Equals("downLoad"))
                return Json(jdata.modelToJqgridResult(A58Data)); //下載查詢資料
            return Json(jdata.modelToJqgridResult(A59Data)); //上傳查詢資料
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetA59Excel()
        {
            MSGReturnModel result = new MSGReturnModel();
            result.RETURN_FLAG = false;

            if (Cache.IsSet(CacheList.A58DbMissfileData))
            {
                var A59 = Excel_DownloadName.A59.ToString();
                var A58Data = (List<A58ViewModel>)Cache.Get(CacheList.A58DbMissfileData);  //從Cache 抓資料
                result = A5Repository.DownLoadExcel(A59, ExcelLocation(A59.GetExelName()), A58Data);
            }
            else
            {
                result.DESCRIPTION = Message_Type.not_Find_Any.GetDescription();
            }
            return Json(result);
        }

        /// <summary>
        /// 選擇檔案後點選資料上傳觸發
        /// </summary>
        /// <param name="FileModel"></param>
        /// <returns>MSGReturnModel</returns>
        [HttpPost]
        public JsonResult UploadA59(ValidateFiles FileModel)
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
                    Excel_UploadName.A59.GetDescription(),
                    pathType); //固定轉成此名稱

                Cache.Invalidate(CacheList.A59ExcelName); //清除 Cache
                Cache.Set(CacheList.A59ExcelName, fileName, 15); //把資料存到 Cache

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
                List<A59ViewModel> dataModel = A5Repository.getA59Excel(pathType, stream);
                if (dataModel.Count > 0)
                {
                    result.RETURN_FLAG = true;
                    var A59 = new A59ViewModel();
                    var jqgridParams = A59.TojqGridData();
                    result.Datas = Json(jqgridParams);
                    Cache.Invalidate(CacheList.A59ExcelfileData); //清除 Cache
                    Cache.Set(CacheList.A59ExcelfileData, dataModel, 15); //把資料存到 Cache
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
    }
}