using System;
using System.Collections.Generic;
using Transfer.Utility;
using Transfer.ViewModels;

namespace Transfer.Models.Interface
{
    public interface ID0Repository
    {
        /// <summary>
        /// get Db Group_Product data by debtType
        /// </summary>
        /// <param name="debtType">1.房貸 4.債券</param>
        /// <returns></returns>
        Tuple<bool, List<GroupProductViewModel>> getGroupProductByDebtType(string debtType);

        /// <summary>
        /// get Db D05 all data
        /// </summary>
        /// <returns></returns>
        Tuple<bool, List<D05ViewModel>> getD05All(string debtType);

        /// <summary>
        /// get Db D05 data
        /// </summary>
        /// <returns></returns>
        Tuple<bool, List<D05ViewModel>> getD05(string debtType, string groupProductCode, string productCode, string processingDate);

        /// <summary>
        /// save D05 To Db
        /// </summary>
        /// <param name="debtType"></param>
        /// <param name="actionType"></param>
        /// <param name="dataModel">D05ViewModel</param>
        /// <returns></returns>
        MSGReturnModel saveD05(string debtType, string actionType, D05ViewModel dataModel);

        /// <summary>
        /// delete D05
        /// </summary>
        /// <param name="productCode">產品</param>
        /// <returns></returns>
        MSGReturnModel deleteD05(string productCode);

        /// <summary>
        /// get Db D01 all data
        /// </summary>
        /// <param name="debtType">1.房貸 4.債券</param>
        /// <returns></returns>
        Tuple<bool, List<D01ViewModel>> getD01All(string debtType);

        /// <summary>
        /// get Db D01 data
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        Tuple<bool, List<D01ViewModel>> getD01(D01ViewModel dataModel);

        /// <summary>
        /// save D01 To Db
        /// </summary>
        /// <param name="dataModel">D01ViewModel</param>
        /// <returns></returns>
        MSGReturnModel saveD01(D01ViewModel dataModel);

        /// <summary>
        /// delete D01
        /// </summary>
        /// <param name="prjid">專案名稱</param>
        /// <param name="flowid">流程名稱</param>
        /// <returns></returns>
        MSGReturnModel deleteD01(string prjid, string flowid);
    }
}