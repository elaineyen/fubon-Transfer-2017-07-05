using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Transfer.Utility;
using Transfer.ViewModels;

namespace Transfer.Models.Interface
{
    public interface IA5Repository
    {
        /// <summary>
        /// get A58 Data
        /// </summary>
        /// <param name="datepicker">ReportDate</param>
        /// <param name="sType">Rating_Type</param>
        /// <param name="from">Origination_Date start</param>
        /// <param name="to">Origination_Date to</param>
        /// <param name="bondNumber">bondNumber</param>
        /// <param name="version">version</param>
        /// <param name="search">全部or缺漏</param>
        /// <returns></returns>
        Tuple<bool, List<A58ViewModel>> GetA58(string datepicker, string sType, string from, string to, string bondNumber, string version, string search);

        /// <summary>
        ///  Excel 資料轉成 A59ViewModel
        /// </summary>
        /// <param name="pathType">Excel 副檔名</param>
        /// <param name="stream"></param>
        /// <returns></returns>
        List<A59ViewModel> getA59Excel(string pathType, Stream stream);

        /// <summary>
        /// get 轉檔紀錄Table 資料
        /// </summary>
        /// <returns></returns>
        List<CheckTableViewModel> getCheckTable();

        /// <summary>
        ///  下載 Excel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">(A59)</param>
        /// <param name="path">下載位置</param>
        /// <param name="cache">cache 資料</param>
        /// <returns></returns>
        MSGReturnModel DownLoadExcel<T>(string type, string path, List<T> data);

        /// <summary>
        /// save A59 To Db 
        /// </summary>
        /// <param name="dataModel">A59ViewModel</param>
        /// <returns></returns>
        MSGReturnModel saveA59(List<A59ViewModel> dataModel);

        /// <summary>
        /// 手動轉換 A57 & A58
        /// </summary>
        /// <param name="date"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        MSGReturnModel saveA57A58(DateTime date, int version);

        void SaveChange();
    }
}