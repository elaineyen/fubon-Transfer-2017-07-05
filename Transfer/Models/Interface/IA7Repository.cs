using System;
using System.Collections.Generic;
using System.IO;
using Transfer.Utility;
using Transfer.ViewModels;

namespace Transfer.Models.Interface
{
    public interface IA7Repository
    {
        /// <summary>
        /// Excel資料 轉 Exhibit29Model
        /// </summary>
        /// <param name="pathType"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        List<Exhibit29Model> getExcel(string pathType, Stream stream);

        /// <summary>
        /// 下載 Excel
        /// </summary>
        /// <param name="type">(A72.A73)</param>
        /// <param name="path">下載位置</param>
        /// <returns></returns>
        MSGReturnModel DownLoadExcel(string type, string path);

        /// <summary>
        /// save A71 To Db
        /// </summary>
        /// <param name="dataModel">Exhibit29Model</param>
        /// <returns></returns>
        MSGReturnModel saveA71(List<Exhibit29Model> dataModel);

        /// <summary>
        /// save A72 To Db
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        MSGReturnModel saveA72();

        /// <summary>
        /// save A73 To Db
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        MSGReturnModel saveA73();

        /// <summary>
        /// save A51 To Db
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        MSGReturnModel saveA51();

        /// <summary>
        /// Get A71 Data
        /// </summary>
        /// <returns></returns>
        Tuple<bool, List<Moody_Tm_YYYY>> GetA71();

        /// <summary>
        /// Get A71 Data
        /// </summary>
        /// <returns></returns>
        Tuple<bool, List<object>> GetA72();

        /// <summary>
        /// Get A73 Data
        /// </summary>
        /// <returns></returns>
        Tuple<bool, List<object>> GetA73();

        /// <summary>
        /// Get A51 Data
        /// </summary>
        /// <returns></returns>
        Tuple<bool, List<Grade_Moody_Info>> GetA51();
    }
}
