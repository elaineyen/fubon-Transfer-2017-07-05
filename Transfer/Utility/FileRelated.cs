﻿using System;
using System.Data;
using System.IO;
using System.Web;
using NPOI;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
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
        public static string DataTableToExcel(DataTable dt, string path, string sheetName)
        {
            string result = string.Empty;

            try
            {

                //建立Excel 2003檔案
                IWorkbook wb = new HSSFWorkbook();
                ISheet ws;

                ////建立Excel 2007檔案
                //IWorkbook wb = new XSSFWorkbook();
                //ISheet ws;


                ws = wb.CreateSheet(sheetName);

                ws.CreateRow(0);//第一行為欄位名稱
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    ws.GetRow(0).CreateCell(i).SetCellValue(dt.Columns[i].ColumnName);
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ws.CreateRow(i + 1);
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (sheetName.IndexOf("A7") > -1) //A7 系列
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
                        else
                        {
                            ws.GetRow(i + 1).CreateCell(j).SetCellValue(dt.Rows[i][j].ToString());
                        }                            
                    }
                }

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
        #endregion
    }
}