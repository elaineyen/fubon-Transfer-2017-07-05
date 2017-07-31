using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Transfer.Utility
{
    public class CheckBoxListInfo
    {
        public string Value { get; set; }
        public string DisplayText { get; set; }
        public bool IsChecked { get; set; }
    }
    public static class Extension
    {

        public static IEnumerable<T> Filter<T>
            (this IEnumerable<T> data , Func<T,bool> fun)
        {
            foreach (T item in data)
            {
                if (fun(item))
                    yield return item; 
            }
        }

        public static bool IsNullOrWhiteSpace
            (this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        public static bool IsNullOrEmpty
             (this string str)
        {
            return string.IsNullOrEmpty(str);
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
        #endregion

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
        #endregion

        public static double GetYears(this TimeSpan timespan)
        {
            return (double)(timespan.Days / 365.2425);
        }

        public static int GetMonths(this TimeSpan timespan)
        {
            return (int)(timespan.Days / 30.436875);
        }

        public static string GetDescription<T>(this T enumerationValue,string title = null,string body = null)
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
        #endregion

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
    }
}