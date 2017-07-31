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
        /// 讀取每一位使用者的 menu
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        List<CheckBoxListInfo> getMenu(string userName);

        /// <summary>
        /// save user menu 設定
        /// </summary>
        /// <param name="menuSub"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        MSGReturnModel saveMenu(List<CheckBoxListInfo> menuSub, string userName);

        /// <summary>
        /// get 可以設定的使用者名稱
        /// </summary>
        /// <returns></returns>
        List<string> getUser();

        void SaveChange();
    }
}
