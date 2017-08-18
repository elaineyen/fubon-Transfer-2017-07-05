using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Transfer.Models.Interface;
using Transfer.Utility;
using static Transfer.Enum.Ref;

namespace Transfer.Models.Repository
{
    public class Common : ICommon
    {
        private IFRS9Entities db = new IFRS9Entities();
        #region save sqllog(IFRS9_Log)
        /// <summary>
        /// Log資料存到Sql(IFRS9_Log)
        /// </summary>
        /// <param name="tableType">table簡寫</param>
        /// <param name="tableName">table名</param>
        /// <param name="fileName">檔案名</param>
        /// <param name="programName">專案名</param>
        /// <param name="falg">成功失敗</param>
        /// <param name="deptType">B:債券 M:房貸 (共用同一table時做區分)</param>
        /// <param name="start">開始時間</param>
        /// <param name="end">結束時間</param>
        /// <returns>回傳成功或失敗</returns>
        public bool saveLog(
            Table_Type table,
            string fileName,
            string programName,
            bool falg,
            string deptType,
            DateTime start,
            DateTime end)
        {
            bool flag = true;
            try
            {
                var tableName = table.GetDescription();
                db.IFRS9_Log.Add(new IFRS9_Log() //寫入DB
                {
                    Table_type = table.ToString(),
                    Table_name = tableName.Substring(0, (tableName.Length > 40 ? 40 : tableName.Length)),
                    File_name = fileName,
                    Program_name = programName,
                    Create_date = start.ToString("yyyyMMdd"),
                    Create_time = start.ToString("HH:mm:ss"),
                    End_date = end.ToString("yyyyMMdd"),
                    End_time = end.ToString("HH:mm:ss"),
                    TYPE = falg ? "Y" : "N",
                    Debt_Type = deptType
                });
                db.SaveChanges(); //DB SAVE
            }
            catch
            {
                flag = false;
            }
            return flag;
        }
        #endregion

        /// <summary>
        /// 轉檔紀錄存到Sql(Transfer_CheckTable)
        /// </summary>
        /// <param name="fileName">檔案名稱 A41,A42...</param>
        /// <param name="falg">成功失敗</param>
        /// <param name="reportDate">基準日</param>
        /// <param name="version">版本</param>
        /// <param name="start">轉檔開始時間</param>
        /// <param name="end">轉檔結束時間</param>
        /// <returns></returns>
        public bool saveTransferCheck(
            string fileName,
            bool falg,
            DateTime reportDate,
            string version,
            DateTime start,
            DateTime end)
        {
            if (db.Transfer_CheckTable.Any(x =>
             x.ReportDate == reportDate &&
             x.Version == version &&
             x.File_Name == fileName))
                return false;
            if (EnumUtil.GetValues<Transfer_Table_Type>()
                .Select(x => x.ToString()).ToList().Contains(fileName))
            {
                db.Transfer_CheckTable.Add(new Transfer_CheckTable()
                {
                    File_Name = fileName,
                    ReportDate = reportDate,
                    Version = version,
                    TransferType = falg ? "Y" : "N",
                    Create_date = start.ToString("yyyyMMdd"),
                    Create_time = start.ToString("HH:mm:ss"),
                    End_date = end.ToString("yyyyMMdd"),
                    End_time = end.ToString("HH:mm:ss"),
                });
                try
                {
                    db.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// 判斷轉檔紀錄是否有存在
        /// </summary>
        /// <param name="fileNames">要判斷的檔案名稱(可多個)</param>
        /// <param name="reportDate">基準日</param>
        /// <param name="version">版本</param>
        /// <returns></returns>
        public bool checkTransferCheck(
            List<string> fileNames,
            DateTime reportDate,
            string version)
        {
            if (!fileNames.Any())
                return false;
            if (db.Transfer_CheckTable
                .Count(x => fileNames.Contains(x.File_Name) &&
                       x.ReportDate == reportDate &&
                       x.Version == version &&
                       "Y".Equals(x.TransferType)) == fileNames.Count)
                return true;
            return false;
        }

        /// <summary>
        /// get EF connection to ADO.NET connection
        /// </summary>
        /// <param name="efConnection"></param>
        /// <returns></returns>
        public string RemoveEntityFrameworkMetadata(string efConnection)
        {
            string efstr = string.Empty;
            if (string.IsNullOrWhiteSpace(efConnection))
            {
                efstr = System.Configuration.ConfigurationManager.
                         ConnectionStrings["IFRS9Entities"].ConnectionString;
            }
            else
            {
                efstr = efConnection;
            }
            int start = efstr.IndexOf("\"", StringComparison.OrdinalIgnoreCase);
            int end = efstr.LastIndexOf("\"", StringComparison.OrdinalIgnoreCase);

            // We do not want to include the quotation marks
            start++;
            int length = end - start;

            string pureSqlConnection = efstr.Substring(start, length);
            return pureSqlConnection.Replace("XXXXX", "1qaz@WSX");
        }
    }
}