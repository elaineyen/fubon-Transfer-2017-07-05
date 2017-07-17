using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        void SaveChange();
    }
}
