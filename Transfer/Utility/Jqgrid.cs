using System;
using System.Collections.Generic;
using System.Linq;

namespace Transfer.Utility
{
    public static class Jqgrid
    {
        public static object modelToJqgridResult<T>(
            this jqGridParam jdata,
            List<T> data)
        {
            if (0.Equals(data.Count))
                return new
                {
                    total = 1,
                    page = 1,
                    records = 0,
                };
            if (jdata._search)
            {
                switch (jdata.searchOper)
                {
                    case "ne": //不等於
                        data = data.Where(x =>
                                typeof(T).GetProperty(jdata.searchField)
                                .GetValue(x, null).ToString()
                                 != jdata.searchString).ToList();
                        break;
                    //case "bw": //開始於
                    //    break;
                    //case "bn": //不開始於
                    //    break;
                    //case "ew": //結束於
                    //    break;
                    //case "en": //不結束於
                    //    break;
                    //case "cn": //包含
                    //    break;
                    //case "nc": //不包含
                    //    break;
                    //case "nu": //is null
                    //    break;
                    //case "nn": //is not null
                    //    break;
                    //case "in": //在其中
                    //    break;
                    //case "ni": //不在其中
                    //    break;
                    case "eq": //等於
                    default:
                        data = data.Where(x =>
                                typeof(T).GetProperty(jdata.searchField)
                                .GetValue(x, null).ToString()
                                .Equals(jdata.searchString)).ToList();
                        break;
                }
            }

            var count = data.Count;
            int pageIndex = jdata.page;
            int pageSize = jdata.rows;
            int totalRecords = count;
            int totalPages = (int)Math.Ceiling((float)totalRecords / (float)pageSize);

            return new
            {
                total = totalPages,
                page = pageIndex,
                records = count,
                rows =
                 !jdata.sidx.IsNullOrWhiteSpace() ?
                 (
                     "asc".Equals(jdata.sord) ?
                     data.Skip((pageIndex - 1) * pageSize).Take(pageSize)
                     .OrderBy(x => typeof(T).GetProperty(jdata.sidx).GetValue(x, null))
                     :
                      data.Skip((pageIndex - 1) * pageSize).Take(pageSize)
                     .OrderByDescending(x => typeof(T).GetProperty(jdata.sidx).GetValue(x, null))
                 ) :
                data.Skip((pageIndex - 1) * pageSize).Take(pageSize)
            };
        }
    }
}