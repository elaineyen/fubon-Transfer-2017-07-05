using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Transfer.Models;
using Transfer.Utility;
using Transfer.ViewModels;

namespace Transfer.Controllers
{
    public class IFRS9Controller : Controller
    {
        private IFRS9Entities db = new IFRS9Entities();
        public ActionResult Index()
        {
            return View();
        }

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

                #region 前端檔案大小不服或不為Excel檔案
                if (FileModel.File.ContentLength == 0 || !ModelState.IsValid)
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = "請確認檔案為Excel檔案或超過大小!";
                    return Json(result);
                }
                #endregion

                #region 上傳檔案
                var fileName = Path.GetFileName(FileModel.File.FileName);

                #region 檢查是否有FileUploads資料夾,如果沒有就新增 並加入 excel 檔案

                string projectFile = Server.MapPath("~/FileUploads");
                string path = Path.Combine(projectFile, fileName);
                bool exists = Directory.Exists(projectFile);
                if (!exists) Directory.CreateDirectory(projectFile);

                using (var fileStream = new FileStream(path,
                    FileMode.Create, FileAccess.ReadWrite))
                {
                    FileModel.File.InputStream.CopyTo(fileStream);
                }
                #endregion

                #region 讀取Excel資料 使用ExcelDataReader 並且組成 json
                string pathType =
                    Path.GetExtension(FileModel.File.FileName)
                    .Substring(1); //檔案類型
                var stream = FileModel.File.InputStream;
                List<ExhibitModel> dataModel = getExcel(pathType, stream);
                if (dataModel.Count > 0)
                {
                    result.RETURN_FLAG = true;
                    result.Datas = Json(dataModel);
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

        [HttpPost]
        public JsonResult Transfer()
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                // Excel 檔案位置
                string projectFile = Server.MapPath("~/FileUploads");
                string path = Path.Combine(projectFile, @"Exhibit 10.xlsx");
                FileStream stream = System.IO.File.Open(path, FileMode.Open, FileAccess.Read);

                string pathType = path.Split('.')[1];
                List<ExhibitModel> dataModel = getExcel(pathType, stream);



            }
            catch (Exception ex)
            {

            }      
            return Json(result);
        }

        #region private function

        #region datarow 組成 ExhibitModel
        private ExhibitModel getExhibitModels(DataRow item)
        {
            DateTime minDate = DateTime.MinValue;
            if (item[0] != null)
                DateTime.TryParse(item[0].ToString(), out minDate);
            return new ExhibitModel()
            {
                Trailing = (item[0] != null) && (minDate != DateTime.MinValue) ?
                minDate.ToString("yyyy/MM/dd") : string.Empty,
                Actual_Allcorp = (item[1] != null) ?
                item[1].ToString() : string.Empty,
                Baseline_forecast_Allcorp = (item[2] != null) ?
                item[2].ToString() : string.Empty,
                Pessimistic_Forecast_Allcorp = (item[3] != null) ?
                item[3].ToString() : string.Empty,
                Actual_SG = (item[4] != null) ?
                item[4].ToString() : string.Empty,
                Baseline_forecast_SG = (item[5] != null) ?
                item[5].ToString() : string.Empty,
                Pessimistic_Forecast_SG = (item[6] != null) ?
                item[6].ToString() : string.Empty
            };
        }
        #endregion

        #region get Excel to List<ExhibitModel>
        private List<ExhibitModel> getExcel(string pathType,Stream stream)
        {
            DataSet resultData = new DataSet();
            List<ExhibitModel> dataModel = new List<ExhibitModel>();
            try
            {
                IExcelDataReader reader = null;
                switch (pathType)
                {
                    case "xls":
                        reader = ExcelReaderFactory.CreateBinaryReader(stream);
                        break;
                    case "xlsx":
                        reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                        break;
                }
                reader.IsFirstRowAsColumnNames = true;
                resultData = reader.AsDataSet();
                reader.Close();
                              
                if (resultData.Tables[0].Rows.Count > 2)
                {
                    dataModel = (from q in resultData.Tables[0].AsEnumerable()
                                 select getExhibitModels(q)).Skip(1).ToList();
                }
            }
            catch
            {}
            return dataModel;
        }
        #endregion

        #endregion


    }
}