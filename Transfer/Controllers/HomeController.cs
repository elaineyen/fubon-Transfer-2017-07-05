using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

using System.Web.Mvc;
using Transfer.Utility;
using Transfer.ViewModels;

namespace Transfer.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Upload(ValidateFiles FileModel)
        {
            MSGReturnModel result = new MSGReturnModel();
            try {
                if (FileModel.File == null)
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = "請選擇檔案!";
                    return Json(result);
                }
                if (FileModel.File.ContentLength > 0 && ModelState.IsValid)
                {
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

                    #region 讀取Excel資料 使用ExcelDataReader

                    IExcelDataReader reader = null;
                    string pathType = 
                        Path.GetExtension(FileModel.File.FileName)
                        .Substring(1); //檔案類型
                    var stream = FileModel.File.InputStream;
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
                    DataSet resultData = reader.AsDataSet();
                    reader.Close();

                    #endregion

                    #region 把Excel資料抓出來並且組成 json

                    List<ExhibitModel> dataModel = new List<ExhibitModel>();
                    if (resultData.Tables[0].Rows.Count > 2)
                    {
                        dataModel = (from q in resultData.Tables[0].AsEnumerable()
                         select getExhibitModels(q)).Skip(1).ToList();
                        result.RETURN_FLAG = true;
                        result.Datas = Json(dataModel);
                    }
                    else
                    {
                        result.RETURN_FLAG = false;
                        result.DESCRIPTION = "無筆對到資料!";
                    }
                    #endregion
                }
                else
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = "請確認檔案為Excel檔案或超過大小!";
                }
            }
            catch (Exception ex){
                result.RETURN_FLAG = false;
                result.DESCRIPTION = ex.Message;
            }

            return Json(result);
        }

        #region private function
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


    }
}