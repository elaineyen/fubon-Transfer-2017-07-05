using Excel;
using System;
using System.Collections.Generic;
using System.Configuration;
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
                #endregion

                #region 讀取Excel資料 使用ExcelDataReader 並且組成 json
                string pathType =
                    Path.GetExtension(FileModel.File.FileName)
                    .Substring(1); //檔案類型
                var stream = FileModel.File.InputStream;
                List<Exhibit29Model> dataModel = getExcel(pathType, stream);
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
                List<Exhibit29Model> dataModel = getExcel(pathType, stream); //Excel轉成 Exhibit10Model

                string proName = "Transfer";
                string tableName = string.Empty;
                #endregion

                #region save Moody_Tm_YYYY(A71)
                tableName = "Moody_Tm_YYYY";
                bool flagA81 = saveA71(dataModel); //save to DB
                //bool A81Log = saveLog(tableName, fileName, proName, flagA71, startTime, DateTime.Now); //寫sql Log
                TxtLog.txtLog(tableName, flagA81, startTime, txtLocation()); //寫txt L7g
                #endregion

                result.RETURN_FLAG = flagA81 && flagA82 && flagA83;
                if (!result.RETURN_FLAG)
                {
                    result.DESCRIPTION = (flagA81 ? string.Empty : "A81 Error! ") +
                                         (flagA82 ? string.Empty : "A82 Error! ") +
                                         (flagA83 ? string.Empty : "A83 Error! ");
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

        #region datarow 組成 Exhibit10Model
        /// <summary>
        /// datarow 組成 Exhibit10Model
        /// </summary>
        /// <param name="item">DataRow</param>
        /// <returns>Exhibit10Model</returns>
        private Exhibit29Model getExhibit29Model(DataRow item)
        {
            return new Exhibit29Model()
            {
               From_To = TypeTransfer.objToString(item[0]),
                Aaa = TypeTransfer.objToString(item[1]),
                Aa1 = TypeTransfer.objToString(item[2]),
                Aa2 = TypeTransfer.objToString(item[3]),
                Aa3 = TypeTransfer.objToString(item[4]),
                A1 = TypeTransfer.objToString(item[5]),
                A2 = TypeTransfer.objToString(item[6]),
                A3 = TypeTransfer.objToString(item[7]),
                Baa1 = TypeTransfer.objToString(item[8]),
                Baa2 = TypeTransfer.objToString(item[9]),
                Baa3 = TypeTransfer.objToString(item[10]),
                Ba1 = TypeTransfer.objToString(item[11]),
                Ba2 = TypeTransfer.objToString(item[12]),
                Ba3 = TypeTransfer.objToString(item[13]),
                B1 = TypeTransfer.objToString(item[14]),
                B2 = TypeTransfer.objToString(item[15]),
                B3 = TypeTransfer.objToString(item[16]),
                Caa1 = TypeTransfer.objToString(item[17]),
                Caa2 = TypeTransfer.objToString(item[18]),
                Caa3 = TypeTransfer.objToString(item[19]),
                Ca_C = TypeTransfer.objToString(item[20]),
                WR = TypeTransfer.objToString(item[21]),
                Default = TypeTransfer.objToString(item[22]),
            };
        }
        #endregion

        #region get Excel to List<Exhibit29Model>
        /// <summary>
        /// 把Excel 資料轉換成 Exhibit29Model
        /// </summary>
        /// <param name="pathType">string</param>
        /// <param name="stream">Stream</param>
        /// <returns>ExhibitModels</returns>
        private List<Exhibit29Model> getExcel(string pathType, Stream stream)
        {
            DataSet resultData = new DataSet();
            List<Exhibit29Model> dataModel = new List<Exhibit29Model>();
            try
            {
                IExcelDataReader reader = null;
                switch (pathType) //判斷型別
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

                if (resultData.Tables[0].Rows.Count > 2) //判斷有無資料
                {
                    dataModel = (from q in resultData.Tables[0].AsEnumerable()
                                 select getExhibit29Model(q)).Skip(1).ToList();
                    //skip(1) 為排除Excel Title列那行(參數可調)
                    dataModel = dataModel.Take(dataModel.Count - 1).ToList();
                    //排除最後一筆 為 * Data in percent 的註解 
                }
            }
            catch
            { }
            return dataModel;
        }
        #endregion
    }
}