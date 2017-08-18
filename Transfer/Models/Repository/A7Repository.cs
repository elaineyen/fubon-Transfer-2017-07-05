using Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Transfer.Enum;
using Transfer.Models.Interface;
using Transfer.Utility;
using Transfer.ViewModels;
using static Transfer.Enum.Ref;

namespace Transfer.Models.Repository
{
    public class A7Repository : IA7Repository, IDbEvent
    {
        #region 其他
        private Common common = new Common();

        private List<string> A73Array = new List<string>() { "TM", "Default" }; //設定 A73要抓的欄位

        protected IFRS9Entities db
        {
            get;
            private set;
        }

        public A7Repository()
        {
            this.db = new IFRS9Entities();
        }

        public void SaveChange()
        {
            db.SaveChanges();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.db != null)
                {
                    this.db.Dispose();
                    this.db = null;
                }
            }
        }
        #endregion

        #region Get Data

        #region Get Moody_Monthly_PD_Info(A71)
        /// <summary>
        /// Get A71 Data
        /// </summary>
        /// <returns></returns>
        public Tuple<bool, List<Moody_Tm_YYYY>> GetA71()
        {
            if (db.Moody_Tm_YYYY.Any())
            {
                return new Tuple<bool, List<Moody_Tm_YYYY>>(true, db.Moody_Tm_YYYY.ToList());
            }
            return new Tuple<bool, List<Moody_Tm_YYYY>>(false, new List<Moody_Tm_YYYY>());
        }
        #endregion

        #region Get Tm_Adjust_YYYY(A72)
        /// <summary>
        /// Get A72 Data
        /// </summary>
        /// <returns></returns>
        public Tuple<bool, List<object>> GetA72()
        {
            if (db.Moody_Tm_YYYY.Any())
            {
                List<object> odatas = new List<object>();
                DataTable datas = getExhibit29ModelFromDb(db.Moody_Tm_YYYY.ToList()).Item1;
                odatas.Add(datas.Columns.Cast<DataColumn>()
                     .Select(x => x.ColumnName)
                     .ToArray()); //第一列 由Columns 組成Title 
                for (var i = 0; i < datas.Rows.Count; i++)
                {
                    List<string> str = new List<string>();
                    for (int j = 0; j < datas.Rows[i].ItemArray.Count(); j++)
                    {
                        if (datas.Columns[j].ToString().IndexOf("TM") > -1)
                        {
                            str.Add("\"" + datas.Columns[j] + "\":\"" + datas.Rows[i].ItemArray[j].ToString() + "\"");
                        }
                        else
                        {
                            str.Add("\"" + datas.Columns[j] + "\":" + datas.Rows[i].ItemArray[j].ToString());
                        }
                        //object 格式為 'column' : Rows.Data
                    }
                    odatas.Add(JsonConvert.DeserializeObject<IDictionary<string, object>>
                        ("{" + string.Join(",", str) + "}")); //第二列以後組成 object
                }
                if (odatas.Count > 2)
                {
                    return new Tuple<bool, List<object>>(true, odatas);
                }
                else
                {
                    return new Tuple<bool, List<object>>(false, new List<object>());
                }
            }
            else
            {
                return new Tuple<bool, List<object>>(false, new List<object>());
            }
        }
        #endregion

        #region Get GM_YYYY(A73)
        /// <summary>
        /// Get A73 Data
        /// </summary>
        /// <returns></returns>
        public Tuple<bool, List<object>> GetA73()
        {
            if (db.Moody_Tm_YYYY.Any())
            {
                List<object> odatas = new List<object>();
                DataTable datas = getExhibit29ModelFromDb(db.Moody_Tm_YYYY.ToList()).Item1;
                odatas.Add(datas.Columns.Cast<DataColumn>()
                     .Where(x => A73Array.Contains(x.ColumnName))
                     .Select(x => x.ColumnName)
                     .ToArray()); //第一列 由Columns 組成Title 
                for (var i = 0; i < datas.Rows.Count; i++)
                {
                    List<string> str = new List<string>();
                    for (int j = 0; j < datas.Rows[i].ItemArray.Count(); j++)
                    {
                        if (A73Array.Contains(datas.Columns[j].ToString()))
                        {
                            if (datas.Columns[j].ToString().IndexOf("TM") > -1)
                            {
                                str.Add("\"" + datas.Columns[j] + "\":\"" + datas.Rows[i].ItemArray[j].ToString() + "\"");
                            }
                            else
                            {
                                str.Add("\"" + datas.Columns[j] + "\":" + datas.Rows[i].ItemArray[j].ToString());
                            }
                        }
                        //object 格式為 'column' : Rows.Data 
                    }
                    odatas.Add(JsonConvert.DeserializeObject<IDictionary<string, object>>
                        ("{" + string.Join(",", str) + "}")); //第二列以後組成 object
                }
                if (odatas.Count > 2)
                {
                    return new Tuple<bool, List<object>>(true, odatas);
                }
                else
                {
                    return new Tuple<bool, List<object>>(false, new List<object>());
                }
            }
            else
            {
                return new Tuple<bool, List<object>>(false, new List<object>());
            }
        }
        #endregion

        #region Get Grade_Moody_Info(A51) 
        /// <summary>
        /// Get A51 Data
        /// </summary>
        /// <returns></returns>
        public Tuple<bool, List<Grade_Moody_Info>> GetA51()
        {
            if (db.Grade_Moody_Info.Any())
            {
                return new Tuple<bool, List<Grade_Moody_Info>>(true, db.Grade_Moody_Info.OrderBy(x => x.PD_Grade).ToList());
            }
            else
            {
                return new Tuple<bool, List<Grade_Moody_Info>>(false, new List<Grade_Moody_Info>());
            }
        }
        #endregion

        #endregion

        #region save Db 

        #region save Moody_Monthly_PD_Info(A71)
        /// <summary>
        /// Save  Moody_Monthly_PD_Info(A71)
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        public MSGReturnModel saveA71(List<Exhibit29Model> dataModel)
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                if (db.Moody_Tm_YYYY.Any())
                    db.Moody_Tm_YYYY.RemoveRange(db.Moody_Tm_YYYY.ToList()); //資料全刪除
                int id = 1;
                foreach (var item in dataModel)
                {
                    db.Moody_Tm_YYYY.Add(
                        new Moody_Tm_YYYY()
                        {
                            Id = id,
                            From_To = item.From_To,
                            Aaa = TypeTransfer.stringToDoubleN(item.Aaa),
                            Aa1 = TypeTransfer.stringToDoubleN(item.Aa1),
                            Aa2 = TypeTransfer.stringToDoubleN(item.Aa2),
                            Aa3 = TypeTransfer.stringToDoubleN(item.Aa3),
                            A1 = TypeTransfer.stringToDoubleN(item.A1),
                            A2 = TypeTransfer.stringToDoubleN(item.A2),
                            A3 = TypeTransfer.stringToDoubleN(item.A3),
                            Baa1 = TypeTransfer.stringToDoubleN(item.Baa1),
                            Baa2 = TypeTransfer.stringToDoubleN(item.Baa2),
                            Baa3 = TypeTransfer.stringToDoubleN(item.Baa3),
                            Ba1 = TypeTransfer.stringToDoubleN(item.Ba1),
                            Ba2 = TypeTransfer.stringToDoubleN(item.Ba2),
                            Ba3 = TypeTransfer.stringToDoubleN(item.Ba3),
                            B1 = TypeTransfer.stringToDoubleN(item.B1),
                            B2 = TypeTransfer.stringToDoubleN(item.B2),
                            B3 = TypeTransfer.stringToDoubleN(item.B3),
                            Caa1 = TypeTransfer.stringToDoubleN(item.Caa1),
                            Caa2 = TypeTransfer.stringToDoubleN(item.Caa2),
                            Caa3 = TypeTransfer.stringToDoubleN(item.Caa3),
                            Ca_C = TypeTransfer.stringToDoubleN(item.Ca_C),
                            WR = TypeTransfer.stringToDoubleN(item.WR),
                            Default_Value = TypeTransfer.stringToDoubleN(item.Default),
                        });
                    id += 1;
                }
                db.SaveChanges(); //Save
                result.RETURN_FLAG = true;
                result.DESCRIPTION = Message_Type.save_Success.GetDescription(Table_Type.A71.ToString());
            }
            catch (DbUpdateException ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type
                                    .save_Fail.GetDescription(Table_Type.A71.ToString(),
                                    $"message: {ex.Message}" +
                                    $", inner message {ex.InnerException?.InnerException?.Message}");
            }
            return result;
        }
        #endregion

        #region Save Tm_Adjust_YYYY(A72)
        /// <summary>
        /// Save  Tm_Adjust_YYYY(A72)
        /// </summary>
        /// <returns></returns>
        public MSGReturnModel saveA72()
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                if (db.Moody_Tm_YYYY.Any())
                {
                    DataTable datas = getExhibit29ModelFromDb(db.Moody_Tm_YYYY.ToList()).Item1;
                    string cs = common.RemoveEntityFrameworkMetadata(string.Empty);
                    using (var conn = new SqlConnection(cs))
                    {
                        using (var cmd = new SqlCommand(CreateA7Table(Table_Type.A72.GetDescription(), datas), conn))
                        {
                            conn.Open();
                            //SqlDataReader reader = cmd.ExecuteReader();
                            //while (reader.Read())
                            //{
                            //    flag = true;
                            //}
                            //reader.Close();
                            int count = cmd.ExecuteNonQuery();
                            if (datas.Rows.Count > 0 && datas.Rows.Count.Equals(count))
                            {
                                result.RETURN_FLAG = true;
                                result.DESCRIPTION = Message_Type.save_Success
                                    .GetDescription(Table_Type.A72.ToString());
                            }
                            else
                            {
                                result.RETURN_FLAG = false;
                                result.DESCRIPTION = Message_Type.save_Fail
                                    .GetDescription(Table_Type.A72.ToString(), "新增筆數有誤!");
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type
                        .save_Fail.GetDescription(Table_Type.A72.ToString(),
                        $"message: {ex.Message}" +
                        $", inner message {ex.InnerException?.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type
                        .save_Fail.GetDescription(Table_Type.A72.ToString(),
                        $"message: {ex.Message}" +
                        $", inner message {ex.InnerException?.InnerException?.Message}");
            }
            return result;
        }
        #endregion

        #region Save GM_YYYY(A73) 
        /// <summary>
        /// Save  GM_YYYY(A73) 
        /// </summary>
        /// <returns></returns>
        public MSGReturnModel saveA73()
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                if (db.Moody_Tm_YYYY.Any())
                {
                    DataTable datas = getExhibit29ModelFromDb(db.Moody_Tm_YYYY.ToList()).Item1;
                    DataTable A73Datas = FromA72GetA73(datas);
                    string cs = common.RemoveEntityFrameworkMetadata(string.Empty);
                    using (var conn = new SqlConnection(cs))
                    {
                        using (var cmd = new SqlCommand(CreateA7Table(Table_Type.A73.GetDescription(), A73Datas), conn))
                        {
                            conn.Open();
                            int count = cmd.ExecuteNonQuery();
                            if (A73Datas.Rows.Count > 0 && A73Datas.Rows.Count.Equals(count))
                            {
                                result.RETURN_FLAG = true;
                                result.DESCRIPTION = Message_Type.save_Success
                                    .GetDescription(Table_Type.A73.ToString());
                            }
                            else
                            {
                                result.RETURN_FLAG = false;
                                result.DESCRIPTION = Message_Type.save_Fail
                                    .GetDescription(Table_Type.A73.ToString(), "新增筆數有誤!");
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type
                        .save_Fail.GetDescription(Table_Type.A73.ToString(),
                        $"message: {ex.Message}" +
                        $", inner message {ex.InnerException?.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type
                        .save_Fail.GetDescription(Table_Type.A73.ToString(),
                        $"message: {ex.Message}" +
                        $", inner message {ex.InnerException?.InnerException?.Message}");
            }
            return result;
        }
        #endregion

        #region Save Grade_Moody_Info(A51) 
        /// <summary>
        /// Save  Grade_Moody_Info(A51) 
        /// </summary>
        /// <returns></returns>
        public MSGReturnModel saveA51()
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                if (db.Moody_Tm_YYYY.Any())
                {
                    if (db.Grade_Moody_Info.Any())
                        db.Grade_Moody_Info.RemoveRange(
                           db.Grade_Moody_Info.ToList()); //資料全刪除  
                    var A51Data = getExhibit29ModelFromDb(db.Moody_Tm_YYYY.ToList());
                    string year = (DateTime.Now.Year - 1).ToString();
                    List<Grade_Moody_Info> A51s = (db.Moody_Tm_YYYY.AsEnumerable().
                        Select((x, y) => new Grade_Moody_Info
                        {
                            Rating = x.From_To,
                            Data_Year = year,
                        })).ToList();
                    A51s.Add(new Grade_Moody_Info() { Rating = "WT", Data_Year = year });
                    A51s.Add(new Grade_Moody_Info() { Rating = "Default", Data_Year = year });
                    int grade_Adjust = 1;
                    int PDGrade = 1;
                    List<string> alreadyNum = new List<string>();
                    foreach (Grade_Moody_Info item in A51s)
                    {
                        string rating_Adjust = string.Empty;
                        foreach (var col in A51Data.Item2)
                        {
                            if (col.Value.Contains(item.Rating))
                            {
                                rating_Adjust = col.Key + "_" + col.Value.Last();
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(rating_Adjust)) //合併欄位情況
                        {
                            if (alreadyNum.Contains(rating_Adjust)) //與上一筆一樣是合併欄位
                            {
                                grade_Adjust -= 1; //Grade_Adjust 不變
                            }
                            else
                            {
                                alreadyNum.Add(rating_Adjust); //新的合併欄位 
                            }
                        }
                        item.Grade_Adjust = grade_Adjust;
                        item.Moodys_PD = string.IsNullOrWhiteSpace(rating_Adjust) ?
                            A51Data.Item1.AsEnumerable().Where(x => x.Field<string>("TM") == item.Rating)
                            .Select(x => Convert.ToDouble(x.Field<string>("Default"))).FirstOrDefault() :
                            A51Data.Item1.AsEnumerable().Where(x => x.Field<string>("TM") == rating_Adjust)
                            .Select(x => Convert.ToDouble(x.Field<string>("Default"))).FirstOrDefault();
                        item.PD_Grade = PDGrade;
                        item.Rating_Adjust = rating_Adjust.Replace("_", "~");
                        grade_Adjust += 1;
                        PDGrade += 1;
                    }
                    db.Grade_Moody_Info.AddRange(A51s);
                    db.SaveChanges(); //Save
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Message_Type.save_Success
                                          .GetDescription(Table_Type.A51.ToString());
                }
            }
            catch (DbUpdateException ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type
                        .save_Fail.GetDescription(Table_Type.A51.ToString(),
                        $"message: {ex.Message}" +
                        $", inner message {ex.InnerException?.InnerException?.Message}");
            }
            return result;
        }
        #endregion

        #endregion

        #region Excel 部分

        #region Excel 資料轉成 Exhibit29Model
        /// <summary>
        /// Excel 資料轉成 Exhibit29Model
        /// </summary>
        /// <param name="pathType">Excel 副檔名</param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public List<Exhibit29Model> getExcel(string pathType, Stream stream)
        {
            DataSet resultData = new DataSet();
            List<Exhibit29Model> dataModel = new List<Exhibit29Model>();
            try
            {
                IExcelDataReader reader = null;
                switch (pathType) //判斷型別
                {
                    case "xls":
                        reader = ExcelReaderFactory.CreateBinaryReader(stream);
                        break;
                    case "xlsx":
                        reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                        break;
                }
                reader.IsFirstRowAsColumnNames = true;
                resultData = reader.AsDataSet();
                reader.Close();

                if (resultData.Tables[0].Rows.Count > 2) //判斷有無資料
                {
                    dataModel = (from q in resultData.Tables[0].AsEnumerable()
                                 select getExhibit29Model(q)).Skip(1).ToList();
                    //skip(1) 為排除Excel Title列那行(參數可調)
                    dataModel = dataModel.Take(dataModel.Count - 1).ToList();
                    //排除最後一筆 為 * Data in percent 的註解 
                }
            }
            catch
            { }
            return dataModel;
        }
        #endregion

        #region 下載 Excel
        /// <summary>
        /// 下載 Excel
        /// </summary>
        /// <param name="type">(A72.A73)</param>
        /// <param name="path">下載位置</param>
        /// <returns></returns>
        public MSGReturnModel DownLoadExcel(string type, string path)
        {
            MSGReturnModel result = new MSGReturnModel();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Message_Type.download_Fail
                .GetDescription(type, "找不到資料");
            if (db.Moody_Tm_YYYY.Any())
            {
                DataTable datas = getExhibit29ModelFromDb(db.Moody_Tm_YYYY.ToList()).Item1;
                switch (type)
                {
                    case "A72":
                        result.DESCRIPTION = FileRelated.DataTableToExcel(datas, path, Table_Type.A72.ToString());
                        result.RETURN_FLAG = string.IsNullOrWhiteSpace(result.DESCRIPTION);
                        break;
                    case "A73":
                        DataTable newData = FromA72GetA73(datas); //要組新的 Table                           
                        if (newData != null) //有資料
                        {
                            result.DESCRIPTION = FileRelated.DataTableToExcel(newData, path, Table_Type.A73.ToString());
                            result.RETURN_FLAG = string.IsNullOrWhiteSpace(result.DESCRIPTION);
                        }
                        else
                        {
                            result.DESCRIPTION = Message_Type.download_Fail.GetDescription(type, "No Data!");
                        }
                        break;
                }
                if (result.RETURN_FLAG)
                    result.DESCRIPTION = Message_Type.download_Success.GetDescription(type);
            }
            return result;
        }
        #endregion

        #endregion

        #region Private Function

        #region datarow 組成 Exhibit29Model
        /// <summary>
        /// datarow 組成 Exhibit29Model
        /// </summary>
        /// <param name="item">DataRow</param>
        /// <returns>Exhibit29Model</returns>
        private Exhibit29Model getExhibit29Model(DataRow item)
        {
            return new Exhibit29Model()
            {
                From_To = TypeTransfer.objToString(item[0]),
                Aaa = TypeTransfer.objToString(item[1]),
                Aa1 = TypeTransfer.objToString(item[2]),
                Aa2 = TypeTransfer.objToString(item[3]),
                Aa3 = TypeTransfer.objToString(item[4]),
                A1 = TypeTransfer.objToString(item[5]),
                A2 = TypeTransfer.objToString(item[6]),
                A3 = TypeTransfer.objToString(item[7]),
                Baa1 = TypeTransfer.objToString(item[8]),
                Baa2 = TypeTransfer.objToString(item[9]),
                Baa3 = TypeTransfer.objToString(item[10]),
                Ba1 = TypeTransfer.objToString(item[11]),
                Ba2 = TypeTransfer.objToString(item[12]),
                Ba3 = TypeTransfer.objToString(item[13]),
                B1 = TypeTransfer.objToString(item[14]),
                B2 = TypeTransfer.objToString(item[15]),
                B3 = TypeTransfer.objToString(item[16]),
                Caa1 = TypeTransfer.objToString(item[17]),
                Caa2 = TypeTransfer.objToString(item[18]),
                Caa3 = TypeTransfer.objToString(item[19]),
                Ca_C = TypeTransfer.objToString(item[20]),
                WR = TypeTransfer.objToString(item[21]),
                Default = TypeTransfer.objToString(item[22]),
            };
        }
        #endregion

        #region DB Moody_Tm_YYYY 組成 DataTable
        /// <summary>
        ///  DB Moody_Tm_YYYY 組成 DataTable
        /// </summary>
        /// <param name="dbDatas"></param>
        /// <returns></returns>
        private Tuple<DataTable, Dictionary<string, List<string>>> getExhibit29ModelFromDb(List<Moody_Tm_YYYY> dbDatas)
        {
            DataTable dt = new DataTable();
            //超過的筆對紀錄 => string 開始的參數,List<string>要相加的參數
            Dictionary<string, List<string>> overData =
                new Dictionary<string, List<string>>();
            try
            {
                #region 找出錯誤的參數
                string errorKey = string.Empty; //錯誤起始欄位
                string last_FromTo = string.Empty; //上一個 From_To
                double last_value = 0d; //上一個default的參數(#)
                foreach (Moody_Tm_YYYY item in dbDatas) //第一次迴圈先抓出不符合的項目
                {
                    double now_default_value =  //目前的default 參數
                        TypeTransfer.doubleNToDouble(item.Default_Value);
                    if (now_default_value >= last_value) //下一筆比上一筆大(正常情況)
                    {
                        if (!errorKey.IsNullOrWhiteSpace()) //假如上一筆是超過的參數
                        {
                            errorKey = string.Empty; //把錯誤Flag 取消掉(到上一筆為止)
                        }
                        #region 把現在的參數寄到最後一個裡面
                        last_FromTo = item.From_To;
                        last_value = now_default_value;
                        #endregion
                    }
                    else //現在的參數比上一個還要小
                    {

                        if (!errorKey.IsNullOrWhiteSpace()) //上一個是錯誤的,修改錯誤記錄資料
                        {
                            var hestory = overData[errorKey];
                            hestory.Add(item.From_To);
                            overData[errorKey] = hestory;
                        }
                        else //上一個是對的(這次錯誤需新增錯誤資料)
                        {
                            overData.Add(last_FromTo,
                                new List<string>() { last_FromTo, item.From_To }); //加入一筆歷史錯誤 
                            errorKey = last_FromTo;//紀錄上一筆的FromTo為超過起始欄位
                        }
                        last_value = (last_value + now_default_value) / 2; //default 相加除以2
                    }
                }
                #endregion

                #region 組出DataTable 的欄位
                dt.Columns.Add("TM", typeof(object)); //第一欄固定為TM
                List<string> errorData = new List<string>(); //錯誤資料
                List<string> rowData = new List<string>(); //左邊行數欄位
                foreach (Moody_Tm_YYYY item in dbDatas) //第二次迴圈組 DataTable 欄位
                {
                    if (overData.ContainsKey(item.From_To)) //為起始錯誤
                    {
                        errorData = overData[item.From_To]; //把錯誤資料找出來
                    }
                    else if (errorData.Contains(item.From_To)) //為中間錯誤
                    {
                        //不做任何動作
                    }
                    else //無錯誤 (columns 加入原本 參數)
                    {
                        if (errorData.Any()) //上一筆是錯誤情形
                        {
                            string key =
                                string.Format("{0}_{1}",
                                errorData.First(),
                                errorData.Last()
                                );
                            dt.Columns.Add(key, typeof(object));
                            errorData = new List<string>();
                            rowData.Add(key);
                        }
                        dt.Columns.Add(item.From_To, typeof(object));
                        rowData.Add(item.From_To);
                    }
                }
                if (errorData.Any()) //此為最後一筆為錯誤時觸發
                {
                    string key =
                        string.Format("{0}_{1}",
                        errorData.First(),
                        errorData.Last()
                        );
                    dt.Columns.Add(key, typeof(double));
                    rowData.Add(key);
                }
                //最後兩欄固定為 WR & Default
                dt.Columns.Add("WR", typeof(string));
                dt.Columns.Add("Default", typeof(string));
                #endregion

                #region 組出資料
                List<string> columnsName = (from q in rowData select q).ToList();
                columnsName.AddRange(new List<string>() { "WR", "Default" });
                foreach (var item in rowData) //by 每一行
                {
                    if (item.IndexOf('_') > -1) //合併行需特別處理
                    {
                        List<string> err = overData[item.Split('_')[0]];
                        List<Moody_Tm_YYYY> dbs = dbDatas.Where(x => err.Contains(x.From_To)).ToList();
                        List<double> datas = new List<double>();
                        foreach (string cname in columnsName) //by 每一欄
                        {
                            if (cname.IndexOf('_') > -1) //合併欄
                            {
                                List<string> err2 = overData[cname.Split('_')[0]];
                                datas.Add((from y in dbs
                                           select (
                                           (from z in err2
                                            select getDbValueINColume(y, z))
                                           .Sum())).Sum() / (err.Count));
                            }
                            else
                            {
                                datas.Add((from x in dbs
                                           select getDbValueINColume(x, cname)).Sum()
                                            / err.Count);
                            }
                        }
                        List<object> o = new List<object>();
                        o.Add(item);
                        o.AddRange((from q in datas select q as object).ToList());
                        var row = dt.NewRow();
                        row.ItemArray = (o.ToArray());
                        dt.Rows.Add(row);
                    }
                    else //其他無合併行的只要單獨處理某特例欄位(合併)
                    {
                        string from_to = item;
                        List<double> datas = new List<double>();
                        Moody_Tm_YYYY db = dbDatas.Where(x => x.From_To == item).First();
                        foreach (string cname in columnsName) //by 每一欄
                        {
                            if (cname.IndexOf('_') > -1) //合併欄
                            {
                                List<string> err = overData[cname.Split('_')[0]];
                                double avg = err.Select(x => getDbValueINColume(db, x)).Sum() / err.Count;
                                datas.Add(avg);
                            }
                            else //正常的
                            {
                                datas.Add(getDbValueINColume(db, cname));
                            }
                        }
                        List<object> o = new List<object>();
                        o.Add(item);
                        o.AddRange((from q in datas select q as object).ToList());
                        var row = dt.NewRow();
                        row.ItemArray = (o.ToArray());
                        dt.Rows.Add(row);
                    }
                }

                //加入 WT & Default 行
                List<string> WTArray = new List<string>() { "Baa1", "Baa2", "Baa3", "Ba1", "Ba2", "Ba3" };
                List<string> WTRow = new List<string>(); //WT要尋找的行的From/To
                foreach (var item in overData) //合併的資料紀錄
                {
                    if (WTArray.Intersect(item.Value).Any()) //找合併裡面符合的
                    {
                        WTRow.Add(item.Key);
                    }
                }
                WTRow.AddRange(rowData.Where(x => WTArray.Contains(x)));
                List<object> WTData = new List<object>();
                List<object> DefaultData = new List<object>();
                WTData.Add("WT");
                DefaultData.Add("Default");
                for (var i = 1; i < dt.Rows[0].ItemArray.Count(); i++) //i從1開始 from/to 那欄不用看
                {
                    double d = 0d;
                    for (var j = 0; j < dt.Rows.Count; j++)
                    {
                        if (WTRow.Contains(dt.Rows[j].ItemArray[0].ToString())) //符合排
                        {
                            d += Convert.ToDouble(dt.Rows[j][i]);
                        }
                    }
                    WTData.Add(d * (dt.Columns[i].ToString().IndexOf("_") > -1 ?
                        overData[dt.Columns[i].ToString().Split('_')[0]].Count : 1)
                        / WTRow.Count); //多筆需*合併數
                    if (i == (dt.Rows[0].ItemArray.Count() - 1))
                    {
                        DefaultData.Add(100d);
                    }
                    else
                    {
                        DefaultData.Add(0d);
                    }
                }
                var nrow = dt.NewRow();
                nrow.ItemArray = (WTData.ToArray());
                dt.Rows.Add(nrow);
                nrow = dt.NewRow();
                nrow.ItemArray = (DefaultData.ToArray());
                dt.Rows.Add(nrow);
                #endregion
            }
            catch 
            {

            }
            return new Tuple<DataTable, Dictionary<string, List<string>>>(dt, overData);
        }
        #endregion

        #region 抓DB的資料
        /// <summary>
        /// 抓DB的資料
        /// </summary>
        /// <param name="db">Moody_Tm_YYYY</param>
        /// <param name="cname">哪一欄位</param>
        /// <returns></returns>
        private double getDbValueINColume(Moody_Tm_YYYY db, string cname)
        {
            if (cname.Equals(A7_Type.Aaa.ToString()))
                return TypeTransfer.doubleNToDouble(db.Aaa);
            if (cname.Equals(A7_Type.Aa1.ToString()))
                return TypeTransfer.doubleNToDouble(db.Aa1);
            if (cname.Equals(A7_Type.Aa2.ToString()))
                return TypeTransfer.doubleNToDouble(db.Aa2);
            if (cname.Equals(A7_Type.Aa3.ToString()))
                return TypeTransfer.doubleNToDouble(db.Aa3);
            if (cname.Equals(A7_Type.A1.ToString()))
                return TypeTransfer.doubleNToDouble(db.A1);
            if (cname.Equals(A7_Type.A2.ToString()))
                return TypeTransfer.doubleNToDouble(db.A2);
            if (cname.Equals(A7_Type.A3.ToString()))
                return TypeTransfer.doubleNToDouble(db.A3);
            if (cname.Equals(A7_Type.Baa1.ToString()))
                return TypeTransfer.doubleNToDouble(db.Baa1);
            if (cname.Equals(A7_Type.Baa2.ToString()))
                return TypeTransfer.doubleNToDouble(db.Baa2);
            if (cname.Equals(A7_Type.Baa3.ToString()))
                return TypeTransfer.doubleNToDouble(db.Baa3);
            if (cname.Equals(A7_Type.Ba1.ToString()))
                return TypeTransfer.doubleNToDouble(db.Ba1);
            if (cname.Equals(A7_Type.Ba2.ToString()))
                return TypeTransfer.doubleNToDouble(db.Ba2);
            if (cname.Equals(A7_Type.Ba3.ToString()))
                return TypeTransfer.doubleNToDouble(db.Ba3);
            if (cname.Equals(A7_Type.B1.ToString()))
                return TypeTransfer.doubleNToDouble(db.B1);
            if (cname.Equals(A7_Type.B2.ToString()))
                return TypeTransfer.doubleNToDouble(db.B2);
            if (cname.Equals(A7_Type.B3.ToString()))
                return TypeTransfer.doubleNToDouble(db.B3);
            if (cname.Equals(A7_Type.Caa1.ToString()))
                return TypeTransfer.doubleNToDouble(db.Caa1);
            if (cname.Equals(A7_Type.Caa2.ToString()))
                return TypeTransfer.doubleNToDouble(db.Caa2);
            if (cname.Equals(A7_Type.Caa3.ToString()))
                return TypeTransfer.doubleNToDouble(db.Caa3);
            if (cname.Equals(A7_Type.Ca_C.ToString()))
                return TypeTransfer.doubleNToDouble(db.Ca_C);
            if (cname.Equals(A7_Type.WR.ToString()))
                return TypeTransfer.doubleNToDouble(db.WR);
            if (cname.Equals(A7_Type.Default.ToString()))
                return TypeTransfer.doubleNToDouble(db.Default_Value);
            return 0d;
        }
        #endregion

        #region A72 資料轉 A73
        /// <summary>
        /// A72 資料轉 A73
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <returns></returns>
        private DataTable FromA72GetA73(DataTable dt)
        {
            DataTable newData = new DataTable(); //要組新的 Table     
            try
            {
                foreach (var itme in A73Array)
                {
                    newData.Columns.Add(itme, typeof(string)); //組 column
                }
                List<string>[] A73datas = new List<string>[A73Array.Count]; //需求的欄位資料
                for (int i = 0; i < A73Array.Count; i++)
                {
                    //取得需求的欄位資料
                    A73datas[i] = dt.AsEnumerable().Select(x => x.Field<string>(A73Array[i])).ToList();
                }
                if (A73datas.Any() && A73datas[0].Any()) //有資料
                {
                    for (int j = 0; j < A73datas[0].Count; j++) //原本datatable 的行數
                    {
                        List<string> o = new List<string>();
                        for (int k = 0; k < A73Array.Count; k++)
                        {
                            o.Add(A73datas[k][j]);
                        }
                        var row = newData.NewRow();
                        row.ItemArray = (o.ToArray());
                        newData.Rows.Add(row);
                    }
                }
            }
            catch
            {

            }
            return newData;
        }
        #endregion

        #region Create Table(DataTable 組 sql Create Table)
        /// <summary>
        /// 動態建Table sql 語法
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        private string CreateA7Table(string tableName, DataTable dt)
        {
            string sqlsc = string.Empty; //create table sql
            sqlsc += string.Format("{0} {1} {2}",
                @" Begin Try drop table ",
                tableName,
                @" End Try Begin Catch End Catch "
                ); //有舊的table 刪除

            sqlsc += " CREATE TABLE " + tableName + "(";
            sqlsc += @" Id INT not null PRIMARY KEY , ";

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                sqlsc += "\n [" + dt.Columns[i].ColumnName.Replace('-', '_')
                    .Replace("Default", "Default_Value") + "] ";
                if (0.Equals(i))
                {
                    sqlsc += " varchar(10) ";
                }
                else
                {
                    sqlsc += " float ";
                }
                sqlsc += " ,";
            }
            sqlsc = sqlsc.Substring(0, sqlsc.Length - 1) + "\n) ";

            string sqlInsert = string.Empty; //insert sql
            int id = 1;
            for (var i = 0; i < dt.Rows.Count; i++) //每一行資料
            {
                string columnArray = string.Format(" {0} ,", "Id");
                string valueArray = string.Format(" {0} ,", id.ToString()); ;
                for (int j = 0; j < dt.Rows[i].ItemArray.Count(); j++)
                {
                    columnArray += string.Format(" {0} ,", dt.Columns[j].ToString()
                        .Replace('-', '_').Replace("Default", "Default_Value"));
                    if (0.Equals(j)) //第一筆是文字
                    {
                        valueArray += string.Format(" '{0}' ,", dt.Rows[i].ItemArray[j].ToString());
                    }
                    else
                    {
                        valueArray += string.Format(" {0} ,", dt.Rows[i].ItemArray[j].ToString());
                    }
                }
                sqlInsert += string.Format(" \n {0} {1} ({2}) {3} ({4}) ",
                             @" insert into ",
                             tableName,
                             columnArray.Substring(0, columnArray.Length - 1),
                             "values",
                             valueArray.Substring(0, valueArray.Length - 1)
                             );
                id += 1;
            }

            return sqlsc + sqlInsert;
        }
        #endregion

        #endregion

    }
}