using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            string tableType,
            string tableName,
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
                int id = 1;
                if (db.IFRS9_Log.Count() > 0) //判斷有無舊的Log
                {
                    id += db.IFRS9_Log.Max(x => x.Id); //Id(Pk) 加一
                }
                db.IFRS9_Log.Add(new IFRS9_Log() //寫入DB
                {
                    Id = id,
                    Table_type = tableType,
                    Table_name = tableName.Substring(0,(tableName.Length > 20 ? 20 : tableName.Length)),
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

        private int _interval = 10000;

        public int Interval
        {
            get { return _interval; }
            set { _interval = value; }
        }
        public bool IsRunning { get; internal set; }

        public void Start(Action fun) 
        {

            if (this.IsRunning)
            {
                return;
            }

            this.IsRunning = true;

            Task.Factory.StartNew(() =>
            {
                while (this.IsRunning)
                {
                    SpinWait.SpinUntil(() => !this.IsRunning, this.Interval);
                    fun();
                }
            });
        }

        public void Stop()
        {
            if (!this.IsRunning)
            {
                return;
            }
            this.IsRunning = false;
        }
    }
}