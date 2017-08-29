using System;
using System.Collections.Generic;
using Transfer.ViewModels;

namespace Transfer.Models.Interface
{
    public interface IC0Repository
    {
        /// <summary>
        /// get Db C07 data
        /// </summary>
        /// <returns></returns>
        Tuple<bool, List<C07ViewModel>> getC07(string debtType, string productCode, string reportDate, string version);
    }
}