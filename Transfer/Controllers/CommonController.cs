using System.Configuration;
using System.IO;
using System.Web.Mvc;
using Transfer.Utility;

namespace Transfer.Controllers
{
    [Authorize]
    public class CommonController : Controller
    {
        protected class SelectOption
        {
            public string text { get; set; }
            public string value { get; set; }
        }

        #region txtlog 設定位置

        protected string txtLocation(string path)
        {
            try
            {
                string projectFile = Server.MapPath("~/" + SetFile.FileUploads); //預設txt位置
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

        #endregion txtlog 設定位置

        #region Excel 設定下載位置

        protected string ExcelLocation(string path)
        {
            try
            {
                string projectFile = Server.MapPath("~/" + SetFile.FileDownloads); //預設Excel下載位置
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

        #endregion Excel 設定下載位置
    }
}