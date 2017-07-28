using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transfer.Models.Interface
{

    public interface ICommon
    {
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
        bool saveLog(
            string tableType,
            string tableName,
            string fileName,
            string programName,
            bool falg,
            string deptType,
            DateTime start,
            DateTime end);
    }
}
