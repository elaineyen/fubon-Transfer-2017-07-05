using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Transfer.Utility
{
    public class Jqgrid
    {
        public static object modelToJqgridResult<T>(
            int totalPages,
            int pageIndex,
            int count,
            List<T> data)
        {
            return new 
                {
                    id = "id",
                    total = totalPages,
                    page = pageIndex,
                    records = count,
                    rows = TypeTransfer.dataToJson(data)
                };

        
        }
    }
}