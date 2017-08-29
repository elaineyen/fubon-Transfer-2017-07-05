using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using Transfer.Enum;
using static Transfer.Enum.Ref;

//using MExcel = Microsoft.Office.Interop.Excel;
namespace Transfer.Utility
{
    public static class FileRelated
    {
        #region 上傳檔案到指定路徑

        /// <summary>
        /// 檔案上傳
        /// </summary>
        /// <param name="path">檔案放置位置</param>
        /// <param name="file">檔案</param>
        /// <returns></returns>
        public static MSGReturnModel FileUpLoadinPath(string path, HttpPostedFileBase file)
        {
            MSGReturnModel result = new MSGReturnModel();
            result.RETURN_FLAG = true;
            try
            {
                using (var fileStream = new FileStream(path,
                     FileMode.Create, FileAccess.ReadWrite))
                {
                    file.InputStream.CopyTo(fileStream); //資料複製一份到FileUploads,存在就覆寫
                }
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Ref.Message_Type.upload_Fail
                    .GetDescription(null, ex.Message);
            }
            return result;
        }

        #endregion 上傳檔案到指定路徑

        #region Create 資料夾

        /// <summary>
        /// Create 資料夾(判斷如果沒有的話就新增)
        /// </summary>
        /// <param name="projectFile">資料夾位置</param>
        public static void createFile(string projectFile)
        {
            try
            {
                bool exists = Directory.Exists(projectFile);
                if (!exists) Directory.CreateDirectory(projectFile);
            }
            catch
            { }
        }

        #endregion Create 資料夾

        #region Download Excel

        /// <summary>
        /// 將 DataTable 資料轉換至 Excel
        /// </summary>
        /// <param name="thisTable">欲轉換之DataTable</param>
        /// <param name="path">檔案放置位置</param>
        /// <param name="sheetName">寫入之sheet名稱</param>
        /// <returns>失敗時回傳錯誤訊息</returns>
        public static string DataTableToExcel(DataTable dt, string path, Excel_DownloadName type)
        {
            string result = string.Empty;

            try
            {
                string version = "2003"; //default 2003
                IWorkbook wb = null;
                ISheet ws;
                string configVersion = ConfigurationManager.AppSettings["ExcelVersion"];
                if (!configVersion.IsNullOrWhiteSpace())
                    version = configVersion;

                //建立Excel 2003檔案
                if ("2003".Equals(version))
                    wb = new HSSFWorkbook();

                if ("2007".Equals(version))
                    wb = new XSSFWorkbook();

                ws = wb.CreateSheet(type.GetDescription());

                ExcelSetValue(ws, dt, type);

                //for (int i = 0; i < dt.Rows.Count; i++)
                //{
                //    ws.CreateRow(i + 1);
                //    for (int j = 0; j < dt.Columns.Count; j++)
                //    {
                //        if (sheetName.IndexOf("A7") > -1) //A7 系列
                //        {
                //            if (0.Equals(j)) //第一行固定為 string
                //            {
                //                ws.GetRow(i + 1).CreateCell(j).SetCellValue(dt.Rows[i][j].ToString());
                //            }
                //            else //後面皆為 double
                //            {
                //                ws.GetRow(i + 1).CreateCell(j).SetCellValue(Convert.ToDouble(dt.Rows[i][j]));
                //            }
                //        }
                //        else
                //        {
                //            ws.GetRow(i + 1).CreateCell(j).SetCellValue(dt.Rows[i][j].ToString());
                //        }
                //    }
                //}

                FileStream file = new FileStream(path, FileMode.Create);//產生檔案
                wb.Write(file);
                file.Close();
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            finally
            {
                //關閉文件
                //oXL.Quit();
            }
            return result;
        }

        #endregion Download Excel

        #region

        private static void ExcelSetValue(ISheet ws, DataTable dt, Excel_DownloadName type)
        {
            ws.CreateRow(0);//第一行為欄位名稱
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                ws.GetRow(0).CreateCell(i).SetCellValue(dt.Columns[i].ColumnName);
            }

            if (type == Excel_DownloadName.A59)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ws.CreateRow(i + 1);
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        ws.GetRow(i + 1).CreateCell(j).SetCellValue(dt.Rows[i][j].ToString());
                    }
                }
            }
            if (type == Excel_DownloadName.A72 || type == Excel_DownloadName.A73)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ws.CreateRow(i + 1);
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (0.Equals(j)) //第一行固定為 string
                        {
                            ws.GetRow(i + 1).CreateCell(j).SetCellValue(dt.Rows[i][j].ToString());
                        }
                        else //後面皆為 double
                        {
                            ws.GetRow(i + 1).CreateCell(j).SetCellValue(Convert.ToDouble(dt.Rows[i][j]));
                        }
                    }
                }
            }
        }

        #endregion
    }
}