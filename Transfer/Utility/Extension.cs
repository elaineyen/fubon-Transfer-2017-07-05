using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Transfer.Utility
{
    public static class EnumUtil
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return System.Enum.GetValues(typeof(T)).Cast<T>();
        }
    }

    public static class Extension
    {
        public static StringBuilder CheckBoxString(string name, List<CheckBoxListInfo> listInfo,
            IDictionary<string, object> htmlAttributes, int number)
        {
            StringBuilder sb = new StringBuilder();
            int lineNumber = 0;
            sb.Append("<table><tr>");
            foreach (CheckBoxListInfo info in listInfo)
            {
                lineNumber++;
                TagBuilder builder = new TagBuilder("input");
                if (info.IsChecked)
                {
                    builder.MergeAttribute("checked", "checked");
                }
                builder.MergeAttributes<string, object>(htmlAttributes);
                builder.MergeAttribute("type", "checkbox");
                builder.MergeAttribute("value", info.Value);
                builder.MergeAttribute("name", name);
                builder.InnerHtml = string.Format(" {0} ", info.DisplayText);
                sb.Append(
                    string.Format("<td>{0}</td>",
                    builder.ToString(TagRenderMode.Normal)));
                if (number == 0)
                {
                    //sb.Append("<br />");
                    sb.Append("</tr><tr>");
                }
                else if (lineNumber % number == 0)
                {
                    //sb.Append("<br />");
                    sb.Append("</tr><tr>");
                }
            }
            if (number == 0 || lineNumber % number == 0)
                sb.Remove(sb.Length - 4, 4);
            else
                sb.Append("</tr>");
            sb.Append("</table>");
            return sb;
        }

        public static IEnumerable<T> Filter<T>
                    (this IEnumerable<T> data, Func<T, bool> fun)
        {
            foreach (T item in data)
            {
                if (fun(item))
                    yield return item;
            }
        }

        public static string GetDescription<T>(this T enumerationValue, string title = null, string body = null)
          where T : struct
        {
            var type = enumerationValue.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException($"{nameof(enumerationValue)} must be of Enum type", nameof(enumerationValue));
            }
            var memberInfo = type.GetMember(enumerationValue.ToString());
            if (memberInfo.Length > 0)
            {
                var attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs.Length > 0)
                {
                    if (!title.IsNullOrWhiteSpace() && !body.IsNullOrWhiteSpace())
                        return string.Format("{0} : {1} => {2}",
                            title,
                            ((DescriptionAttribute)attrs[0]).Description,
                            body
                            );
                    if (!title.IsNullOrWhiteSpace())
                        return string.Format("{0} : {1}",
                            title,
                            ((DescriptionAttribute)attrs[0]).Description
                            );
                    if (!body.IsNullOrWhiteSpace())
                        return string.Format("{0} => {1}",
                            ((DescriptionAttribute)attrs[0]).Description,
                            body
                            );
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            return enumerationValue.ToString();
        }

        public static string GetExelName(this string str)
        {
            if (str.IsNullOrWhiteSpace())
                return string.Empty;
            string version = "2003"; //default 2003
            string configVersion = ConfigurationManager.AppSettings["ExcelVersion"];
            if (!configVersion.IsNullOrWhiteSpace())
                version = configVersion;
            return "2003".Equals(version) ? str + ".xls" : str + ".xlsx";
        }

        public static int GetMonths(this TimeSpan timespan)
        {
            return (int)(timespan.Days / 30.436875);
        }

        public static double GetYears(this TimeSpan timespan)
        {
            return (double)(timespan.Days / 365.2425);
        }

        public static bool IsNullOrEmpty
             (this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsNullOrWhiteSpace
                                    (this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        #region 時間相減 取年

        /// <summary>
        /// 時間相減 取年
        /// </summary>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <returns></returns>
        public static double? dateSubtractToYear(this DateTime? date1, DateTime? date2)
        {
            if (!date1.HasValue || !date2.HasValue)
            {
                return null;
            }
            TimeSpan t = date1.Value.Subtract(date2.Value);
            return t.GetYears();
        }

        #endregion 時間相減 取年

        #region 時間相減 取月

        /// <summary>
        /// 時間相減 取月
        /// </summary>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <returns></returns>
        public static int? dateSubtractToMonths(this DateTime? date1, DateTime? date2)
        {
            if (!date1.HasValue || !date2.HasValue)
            {
                return null;
            }
            TimeSpan t = date1.Value.Subtract(date2.Value);
            return t.GetMonths();
        }

        #endregion 時間相減 取月

        #region CheckBoxList

        /// <summary>
        /// CheckBoxList.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="name">The name.</param>
        /// <param name="listInfo">CheckBoxListInfo.</param>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <returns></returns>
        public static IHtmlString CheckBoxList(this HtmlHelper htmlHelper, string name, List<CheckBoxListInfo> listInfo, object htmlAttributes)
        {
            return htmlHelper.CheckBoxList
            (
                name,
                listInfo,
                (IDictionary<string, object>)new RouteValueDictionary(htmlAttributes),
                0
            );
        }

        /// <summary>
        /// CheckBoxList.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="name">The name.</param>
        /// <param name="listInfo">The list info.</param>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="number">每個Row的顯示個數.</param>
        /// <returns></returns>
        public static IHtmlString CheckBoxList(this HtmlHelper htmlHelper, string name, List<CheckBoxListInfo> listInfo,
            IDictionary<string, object> htmlAttributes, int number)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException("必須給這些CheckBoxList一個Tag Name", "name");
            }
            if (listInfo == null)
            {
                //return htmlHelper.Raw(string.Empty);
                throw new ArgumentNullException("必須要給List<CheckBoxListInfo> listInfo");
            }
            if (listInfo.Count < 1)
            {
                throw new ArgumentException("List<CheckBoxListInfo> listInfo 至少要有一組資料", "listInfo");
            }
            StringBuilder sb = CheckBoxString(name, listInfo, htmlAttributes, number);
            return htmlHelper.Raw(sb.ToString());
        }

        #endregion CheckBoxList

        #region List to DataTable

        public static DataTable ToDataTable<T>(this List<T> items)
            where T : class
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        #endregion List to DataTable

        public static jqGridData<T> TojqGridData<T>(this T cls, int[] widths = null, bool act = false)
            where T : class
        {
            var obj =  cls.GetType();
            if (!obj.IsClass)
                return new jqGridData<T>();
            var jqgridParams = new jqGridData<T>();
            bool flag = false;
            int len = 0;
            if (widths != null && widths.Length > 0)
            {
                len = widths.Length;
                flag = true;
            }              
            int widthIndex = 0;
            int? widthParam = null;
            if (act)
            {
                jqgridParams.colNames.Add("act");
                jqgridParams.colModel.Add(new jqGridColModel()
                {
                    name = "act",
                    index = "act",
                    width = 100
                });
            }
            obj.GetProperties()
                .ToList().ForEach(x =>
                {
                    var str = x.Name;
                    jqgridParams.colNames.Add(str);
                    jqgridParams.colModel.Add(new jqGridColModel()
                    {
                        name = str,
                        index = str,
                        width = flag ? (len > widthIndex ? widths[widthIndex] : widthParam) : widthParam
                    });
                    widthIndex += 1;
                });
            return jqgridParams;
        }
    }

    public class CheckBoxListInfo
    {
        public string DisplayText { get; set; }
        public bool IsChecked { get; set; }
        public string Value { get; set; }
    }
}