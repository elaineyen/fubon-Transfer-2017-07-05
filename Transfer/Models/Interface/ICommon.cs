using System;
using System.Collections.Generic;
using static Transfer.Enum.Ref;

namespace Transfer.Models.Interface
{
    public interface ICommon
    {
        /// <summary>
        /// 判斷轉檔紀錄是否有存在
        /// </summary>
        /// <param name="fileNames">目前檔案名稱</param>
        /// <param name="checkName">要檢查的檔案名稱</param>
        /// <param name="reportDate">基準日</param>
        /// <param name="version">版本</param>
        /// /// <returns></returns>
        bool checkTransferCheck(
            string fileName,
            string checkName,
            DateTime reportDate,
            int version);

        /// <summary>
        /// Log資料存到Sql(IFRS9_Log)
        /// </summary>
        /// <param name="Table_Type">table類型</param>
        /// <param name="fileName">檔案名</param>
        /// <param name="programName">專案名</param>
        /// <param name="falg">成功失敗</param>
        /// <param name="deptType">B:債券 M:房貸 (共用同一table時做區分)</param>
        /// <param name="start">開始時間</param>
        /// <param name="end">結束時間</param>
        /// <returns>回傳成功或失敗</returns>
        bool saveLog(
            Table_Type table,
            string fileName,
            string programName,
            bool falg,
            string deptType,
            DateTime start,
            DateTime end);

        /// <summary>
        /// 轉檔紀錄存到Sql(Transfer_CheckTable)
        /// </summary>
        /// <param name="fileName">檔案名稱 A41,A42...</param>
        /// <param name="falg">成功失敗</param>
        /// <param name="reportDate">基準日</param>
        /// <param name="version">版本</param>
        /// <param name="start">轉檔開始時間</param>
        /// <param name="end">轉檔結束時間</param>
        /// <returns>回傳成功或失敗</returns>
        bool saveTransferCheck(
             string fileName,
             bool falg,
             DateTime reportDate,
             int version,
             DateTime start,
             DateTime end);
    }
}