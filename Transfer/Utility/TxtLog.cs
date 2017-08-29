using System;
using System.IO;
using static Transfer.Enum.Ref;

namespace Transfer.Utility
{
    public static class TxtLog
    {
        #region save txtlog

        /// <summary>
        /// 寫入 Txt Log
        /// </summary>
        /// <param name="tableName">table名</param>
        /// <param name="falg">成功或失敗</param>
        /// <param name="start">開始時間</param>
        /// <param name="folderPath">檔案路徑</param>
        public static void txtLog(Table_Type tableName, bool falg, DateTime start, string folderPath)
        {
            try
            {
                string txtData = string.Empty;
                try //試著抓取舊資料
                {
                    txtData = File.ReadAllText(folderPath, System.Text.Encoding.Default);
                }
                catch { }
                string txt = string.Format("{0}({1})_{2}_{3}",
                             tableName.GetDescription(),
                             tableName.ToString(),
                             start.ToString("yyyyMMddHHmmss"),
                             falg ? "Y" : "N");
                if (!string.IsNullOrWhiteSpace(txtData)) //有舊資料就換行寫入下一筆
                {
                    txtData += string.Format("\r\n{0}", txt);
                }
                else //沒有就直接寫入
                {
                    txtData = txt;
                }
                FileStream fs = new FileStream(folderPath, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);
                sw.Write(txtData); //存檔
                sw.Close();
            }
            catch (Exception ex)
            {
            }
        }

        #endregion save txtlog
    }
}