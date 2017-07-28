using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transfer.Utility;
using Transfer.ViewModels;

namespace Transfer.Models.Interface
{
    public interface ISystemRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        List<CheckBoxListInfo> getMenu(string userName);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="menuSub"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        bool saveMenu(List<CheckBoxListInfo> menuSub, string userName);
        void SaveChange();
    }
}
