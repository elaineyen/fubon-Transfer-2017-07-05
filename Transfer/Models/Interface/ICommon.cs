using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transfer.Models.Interface
{
    public interface ICommon
    {
        bool saveLog(
            string tableName,
            string fileName,
            string programName,
            bool falg,
            DateTime start,
            DateTime end);
    }
}
