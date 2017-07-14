using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Transfer.Models.Interface;

namespace Transfer.Models.Repositiry
{
    public class Common : ICommon
    {
        private IFRS9Entities db = new IFRS9Entities();
        #region save sqllog(IFRS9_Log)
        /// <summary>
        /// Log資料存到Sql(IFRS9_Log)
        /// </summary>
        /// <param name="tableName">table名</param>
        /// <param name="fileName">檔案名</param>
        /// <param name="programName">專案名</param>
        /// <param name="falg">成功失敗</param>
        /// <param name="start">開始時間</param>
        /// <param name="end">結束時間</param>
        /// <returns>回傳成功或失敗</returns>
        public bool saveLog(
            string tableName,
            string fileName,
            string programName,
            bool falg,
            DateTime start,
            DateTime end)
        {
            bool flag = true;
            try
            {
                int id = 1;
                if (db.IFRS9_Log.Count() > 0) //判斷有無舊的Log
                {
                    id += db.IFRS9_Log.Max(x => x.Id); //Id(Pk) 加一
                }
                db.IFRS9_Log.Add(new IFRS9_Log() //寫入DB
                {
                    Id = id,
                    Table_name = tableName.Substring(0, 20),
                    File_name = fileName,
                    Program_name = programName,
                    Create_date = start.ToString("yyyyMMdd"),
                    Create_time = start.ToString("HHmmss"),
                    End_date = end.ToString("yyyyMMdd"),
                    End_time = end.ToString("HH:mm:ss"),
                    TYPE = falg ? "Y" : "N"
                });
                db.SaveChanges(); //DB SAVE
            }
            catch (Exception ex)
            {
                flag = false;
            }
            return flag;
        }
        #endregion

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
            return pureSqlConnection;
        }
    }
}