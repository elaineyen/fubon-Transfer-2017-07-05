using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transfer.Utility;
using Transfer.ViewModels;

namespace Transfer.Models.Interface
{
    public interface IA7Repository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathType"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        List<Exhibit29Model> getExcel(string pathType, Stream stream);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        MSGReturnModel DownLoadExcel(string type, string path);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        MSGReturnModel saveA7(List<Exhibit29Model> dataModel);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Tuple<bool, List<Moody_Tm_YYYY>> GetA71();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Tuple<bool, List<object>> GetA72();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Tuple<bool, List<object>> GetA73();
    }
}
