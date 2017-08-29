using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Transfer.Utility;
using Transfer.ViewModels;

namespace Transfer.Models.Interface
{
    public interface IA5Repository
    {
        Tuple<bool, List<A58ViewModel>> GetA58(string datepicker, string sType, string from, string to, string bondNumber, string version, string search);

        List<A59ViewModel> getA59Excel(string pathType, Stream stream);

        MSGReturnModel DownLoadExcel<T>(string type, string path, List<T> data);

        void SaveChange();
    }
}