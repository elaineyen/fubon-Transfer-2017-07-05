using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;
using Transfer.Models;
using Transfer.Utility;

namespace Transfer.Controllers
{
    public class CommonController : Controller
    {
        #region txtlog 設定位置
        public string txtLocation(string path)
        {
            try
            {
                string projectFile = Server.MapPath("~/FileUploads"); //預設txt位置
                string configTxtLocation = ConfigurationManager.AppSettings["txtLogLocation"];
                if (!string.IsNullOrWhiteSpace(configTxtLocation))
                    projectFile = configTxtLocation; //有設定webConfig且不為空就取代
                FileRelated.createFile(projectFile);
                string folderPath = Path.Combine(projectFile, path); //合併路徑&檔名
                return folderPath;
            }
            catch
            {
                return string.Empty;
            }
        }
        #endregion

        #region Excel 設定下載位置
        public string ExcelLocation(string path)
        {
            try
            {
                string projectFile = Server.MapPath("~/FileDownloads"); //預設Excel下載位置
                string configExcelLocation = ConfigurationManager.AppSettings["ExcelDlLocation"];
                if (!string.IsNullOrWhiteSpace(configExcelLocation))
                    projectFile = configExcelLocation; //有設定webConfig且不為空就取代
                FileRelated.createFile(projectFile);
                string folderPath = Path.Combine(projectFile, path); //合併路徑&檔名
                return folderPath;
            }
            catch
            {
                return string.Empty;
            }
        }
        #endregion
    }
}