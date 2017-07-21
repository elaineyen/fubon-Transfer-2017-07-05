using System;
using System.Collections.Generic;
using System.IO;
using Transfer.Utility;
using Transfer.ViewModels;

namespace Transfer.Models.Interface
{
    public interface IA4Repository
    {
        /// <summary>
        /// Excel資料 轉 Exhibit29Model
        /// </summary>
        /// <param name="pathType"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        List<A41ViewModel> getExcel(string pathType, Stream stream);

        /// <summary>
        /// get Db A41 data
        /// </summary>
        /// <returns></returns>
        Tuple<bool, List<A41ViewModel>> GetA41();

        /// <summary>
        /// save A41 To Db
        /// </summary>
        /// <param name="dataModel">A41ViewModel</param>
        /// <returns></returns>
        MSGReturnModel saveA41(List<A41ViewModel> dataModel);

        /// <summary>
        /// save B01 to DB
        /// </summary>
        /// <param name="version">version</param>
        /// <param name="date">Report_Date</param>
        /// <returns></returns>
        MSGReturnModel saveB01(string version, DateTime date);

        /// <summary>
        /// save C01 to DB
        /// </summary>
        /// <param name="version">version</param>
        /// <param name="date">Report_Date</param>
        /// <returns></returns>
        MSGReturnModel saveC01(string version, DateTime date);

        /// <summary>
        /// 抓取指定的 log 資料
        /// </summary>
        /// <param name="tableTypes">"B01","C01"...</param>
        /// <returns></returns>
        List<string> GetLogData(List<string> tableTypes);

        /// <summary>
        /// 
        /// </summary>
        void SaveChange();
    }
}
