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
        /// 
        /// </summary>
        void SaveChange();

        /// <summary>
        /// 暫存A41資料 在cache
        /// </summary>
        /// <param name="dataModel"></param>
        void saveTempA41(List<A41ViewModel> dataModel);

        /// <summary>
        /// 抓取暫存A41資料 
        /// </summary>
        /// <returns></returns>
        List<A41ViewModel> tempA41();
    }
}
