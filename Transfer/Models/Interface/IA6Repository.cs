using System;
using System.Collections.Generic;
using System.IO;
using Transfer.Utility;
using Transfer.ViewModels;

namespace Transfer.Models.Interface
{
    public interface IA6Repository
    {
        /// <summary>
        /// Get A62 Data
        /// </summary>
        /// <param name="year">搜尋年分 ALL(全部)</param>
        /// <returns></returns>
        Tuple<bool, List<Exhibit7Model>> GetA62(string year);

        /// <summary>
        /// Get A62 Search Year
        /// </summary>
        /// <returns></returns>
        List<string> GetA62SearchYear();

        /// <summary>
        /// Excel資料 轉 Exhibit29Model
        /// </summary>
        /// <param name="pathType"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        List<Exhibit7Model> getExcel(string pathType, Stream stream);

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        MSGReturnModel saveA61();

        /// <summary>
        /// save A62 To Db
        /// </summary>
        /// <param name="dataModel">Exhibit7Model</param>
        /// <returns></returns>
        MSGReturnModel saveA62(List<Exhibit7Model> dataModel);
    }
}