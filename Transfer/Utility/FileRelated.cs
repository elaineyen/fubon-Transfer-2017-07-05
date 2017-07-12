using System;
using System.Data;
using System.IO;
using System.Web;
using MExcel = Microsoft.Office.Interop.Excel;
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
                result.DESCRIPTION = ex.Message;
            }
            return result;
        }
        #endregion

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
        #endregion

        #region Download Excel
        /// <summary>
        /// 將 DataTable 資料轉換至 Excel
        /// </summary>
        /// <param name="thisTable">欲轉換之DataTable</param>
        /// <param name="path">檔案放置位置</param>
        /// <param name="sheetName">寫入之sheet名稱</param>
        public static bool DataTableToExcel(DataTable table, string path, string sheetName)
        {
            bool flag = true;
            //需加入參考
            //References右鍵AddReferences => COM => Microsoft Excel 10.0 Object Library
            //在References會多Excel及Microsoft.Office.Core
            MExcel.Application oXL = null; //引用EXCEL Application類別
            MExcel._Worksheet oSheet = null; //引用工作表類別

            try
            {

                oXL = new MExcel.Application(); // load excel

                oXL.Workbooks.Add(); //create a new workbook(活頁簿)

                oSheet = oXL.ActiveSheet; //single worksheet    

                //int sheetRowsCount = oSheet.UsedRange.Rows.Count;

                // column headings
                for (var i = 0; i < table.Columns.Count; i++)
                {
                    oSheet.Cells[1, i + 1] = table.Columns[i].ColumnName;
                }

                // rows
                for (var i = 0; i < table.Rows.Count; i++)
                {
                    // to do: format datetime values before printing
                    for (var j = 0; j < table.Columns.Count; j++)
                    {
                        oSheet.Cells[i + 2, j + 1] = table.Rows[i][j];
                    }
                }

                oSheet.SaveAs(path);
                oXL.Quit();
           
            }
            catch (Exception ex)
            {
                flag = false;
            }
            finally
            {
                //關閉文件
                oXL.Quit();
            }
            return flag;
        }
        #endregion
    }
}