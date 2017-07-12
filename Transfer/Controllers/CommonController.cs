using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;
using Transfer.Models;
using Transfer.Utility;

namespace Transfer.Controllers
{
    public class CommonController : Controller
    {
        private IFRS9Entities db = new IFRS9Entities();
        #region save sqllog(IFRS9_Log)
        /// <summary>
        /// Log資料存到Sql(IFRS9_Log)
        /// </summary>
        /// <param name="tableName">table名</param>
        /// <param name="fileName">檔案名</param>
        /// <param name="programName">專案名</param>
        /// <param name="falg">成功失敗</param>
        /// <param name="start">開始時間</param>
        /// <param name="end">結束時間</param>
        /// <returns>回傳成功或失敗</returns>
        public bool saveLog(
            string tableName,
            string fileName,
            string programName,
            bool falg,
            DateTime start,
            DateTime end)
        {
            bool flag = true;
            try
            {
                int id = 1;
                if (db.IFRS9_Log.Count() > 0) //判斷有無舊的Log
                {
                    id += db.IFRS9_Log.Max(x => x.Id); //Id(Pk) 加一
                }
                db.IFRS9_Log.Add(new IFRS9_Log() //寫入DB
                {
                    Id = id,
                    Table_name = tableName.Substring(0, 20),
                    File_name = fileName,
                    Program_name = programName,
                    Create_date = start.ToString("yyyyMMdd"),
                    Create_time = start.ToString("HHmmss"),
                    End_date = end.ToString("yyyyMMdd"),
                    End_time = end.ToString("HHmmss"),
                    TYPE = falg ? "Y" : "N"
                });
                db.SaveChanges(); //DB SAVE
            }
            catch (Exception ex)
            {
                flag = false;
            }
            return flag;
        }
        #endregion

        #region txtlog 設定位置
        public string txtLocation(string path)
        {
            try
            {
                string projectFile = Server.MapPath("~/FileUploads"); //預設txt位置
                string configTxtLocation = ConfigurationManager.AppSettings["txtLogLocation"];
                if (!string.IsNullOrWhiteSpace(configTxtLocation))
                    projectFile = configTxtLocation; //有設定webConfig且不為空就取代
                FileUpLoad.createFile(projectFile);
                string folderPath = Path.Combine(projectFile, path); //合併路徑&檔名
                return folderPath;
            }
            catch
            {
                return string.Empty;
            }
        }
        #endregion

        public static string Serialize(object o)
        {
            XmlSerializer ser = new XmlSerializer(o.GetType());
            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb);
            ser.Serialize(writer, o);
            return sb.ToString();
        }

        public static T Deserialize<T>(string s)
        {
            XmlDocument xdoc = new XmlDocument();
            try
            {
                xdoc.LoadXml(s);
                XmlNodeReader reader = new XmlNodeReader(xdoc.DocumentElement);
                XmlSerializer ser = new XmlSerializer(typeof(T));
                object obj = ser.Deserialize(reader);

                return (T)obj;
            }
            catch
            {
                return default(T);
            }
        }
    }
}