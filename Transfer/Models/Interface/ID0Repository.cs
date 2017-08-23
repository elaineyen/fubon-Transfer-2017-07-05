using System;
using System.Collections.Generic;
using Transfer.Utility;
using Transfer.ViewModels;

namespace Transfer.Models.Interface
{
    public interface ID0Repository
    {
        /// <summary>
        /// delete D05
        /// </summary>
        /// <param name="productCode">產品</param>
        /// <returns></returns>
        MSGReturnModel deleteD05(string productCode);

        /// <summary>
        /// get Db D05 data
        /// </summary>
        /// <returns></returns>
        Tuple<bool, List<D05ViewModel>> getD05(string groupProductCode, string groupProduct, string productCode, string processingDate);

        /// <summary>
        /// get Db D05 all data
        /// </summary>
        /// <returns></returns>
        Tuple<bool, List<D05ViewModel>> getD05All();

        /// <summary>
        /// save D05 To Db
        /// </summary>
        /// <param name="dataModel">D05ViewModel</param>
        /// <returns></returns>
        MSGReturnModel saveD05(string actionType, D05ViewModel dataModel);
    }
}