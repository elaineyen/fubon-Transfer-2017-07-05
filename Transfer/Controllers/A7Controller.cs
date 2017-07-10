using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Transfer.Models;
using Transfer.Utility;
using Transfer.ViewModels;

namespace Transfer.Controllers
{
    public class A7Controller : Controller
    {
        private IFRS9Entities db = new IFRS9Entities();

        // GET: A7
        public ActionResult Index()
        {
            ViewBag.Manu = "A7Main";
            ViewBag.SubManu = "A70SubMain";          
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
                //using (var fileStream = new FileStream(path,
                //    FileMode.Create, FileAccess.ReadWrite))
                //{
                //    FileModel.File.InputStream.CopyTo(fileStream); //資料複製一份到FileUploads,存在就覆寫
                //}
                #endregion

                #region 讀取Excel資料 使用ExcelDataReader 並且組成 json
                string pathType =
                    Path.GetExtension(FileModel.File.FileName)
                    .Substring(1); //檔案類型
                var stream = FileModel.File.InputStream;
                //List<ExhibitModel> dataModel = getExcel(pathType, stream);
                //if (dataModel.Count > 0)
                //{
                //    result.RETURN_FLAG = true;
                //    result.Datas = Json(dataModel); //給JqGrid 顯示
                //}
                //else
                //{
                //    result.RETURN_FLAG = false;
                //    result.DESCRIPTION = "無筆對到資料!";
                //}
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

        #region get Excel to List<ExhibitModel>
        /// <summary>
        /// 把Excel 資料轉換成 ExhibitModel
        /// </summary>
        /// <param name="pathType">string</param>
        /// <param name="stream">Stream</param>
        /// <returns>ExhibitModels</returns>
        //private List<ExhibitModel> getExcel(string pathType, Stream stream)
        //{
        //    DataSet resultData = new DataSet();
        //    List<ExhibitModel> dataModel = new List<ExhibitModel>();
        //    try
        //    {
        //        IExcelDataReader reader = null;
        //        switch (pathType) //判斷型別
        //        {
        //            case "xls":
        //                reader = ExcelReaderFactory.CreateBinaryReader(stream);
        //                break;
        //            case "xlsx":
        //                reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
        //                break;
        //        }
        //        reader.IsFirstRowAsColumnNames = true;
        //        resultData = reader.AsDataSet();
        //        reader.Close();

        //        if (resultData.Tables[0].Rows.Count > 2) //判斷有無資料
        //        {
        //            dataModel = (from q in resultData.Tables[0].AsEnumerable()
        //                         select getExhibitModels(q)).Skip(1).ToList();
        //            //skip(1) 為排除Excel Title列那行(參數可調)
        //        }
        //    }
        //    catch
        //    { }
        //    return dataModel;
        //}
        #endregion
    }
}