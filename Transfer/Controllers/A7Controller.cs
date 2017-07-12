using Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Transfer.Enum;
using Transfer.Models;
using Transfer.Utility;
using Transfer.ViewModels;

namespace Transfer.Controllers
{
    public class A7Controller : CommonController
    {
        private IFRS9Entities db = new IFRS9Entities();
        private List<string> A73Array = new List<string>() { "TM", "Default" };

        // GET: A7
        public ActionResult Index()
        {
            ViewBag.Manu = "A7Main";
            ViewBag.SubManu = "A70SubMain";
            return View();
        }

        public ActionResult A71Detail()
        {
            ViewBag.Manu = "A7Main";
            ViewBag.SubManu = "A71SubMain";
            return View();
        }

        public ActionResult A72Detail()
        {
            ViewBag.Manu = "A7Main";
            ViewBag.SubManu = "A72SubMain";
            return View();
        }

        public ActionResult A73Detail()
        {
            ViewBag.Manu = "A7Main";
            ViewBag.SubManu = "A73SubMain";
            return View();
        }

        /// <summary>
        /// 選擇檔案後點選資料上傳觸發
        /// </summary>
        /// <param name="FileModel"></param>
        /// <returns>MSGReturnModel</returns>
        [HttpPost]
        public JsonResult Upload(ValidateFiles FileModel)
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                #region 前端無傳送檔案進來
                if (FileModel.File == null)
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = "請選擇檔案!";
                    return Json(result);
                }
                #endregion

                #region 前端檔案大小不服或不為Excel檔案(驗證)
                if (FileModel.File.ContentLength == 0 || !ModelState.IsValid)
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = "請確認檔案為Excel檔案或超過大小!";
                    return Json(result);
                }
                #endregion

                #region 上傳檔案
                var fileName = Path.GetFileName(FileModel.File.FileName); //檔案名稱

                #region 檢查是否有FileUploads資料夾,如果沒有就新增 並加入 excel 檔案

                string projectFile = Server.MapPath("~/FileUploads"); //專案資料夾
                string path = Path.Combine(projectFile, fileName);
                FileRelated.createFile(projectFile); //檢查是否有FileUploads資料夾,如果沒有就新增

                //呼叫上傳檔案 function
                result = FileRelated.FileUpLoadinPath(path, FileModel.File);
                if (!result.RETURN_FLAG)
                    return Json(result);
                #endregion

                #region 讀取Excel資料 使用ExcelDataReader 並且組成 json
                string pathType =
                    Path.GetExtension(FileModel.File.FileName)
                    .Substring(1); //檔案類型
                var stream = FileModel.File.InputStream;
                List<Exhibit29Model> dataModel = getExcel(pathType, stream);
                if (dataModel.Count > 0)
                {
                    result.RETURN_FLAG = true;
                    result.Datas = Json(dataModel); //給JqGrid 顯示
                }
                else
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = "無筆對到資料!";
                }
                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = ex.Message;
            }
            return Json(result);
        }

        /// <summary>
        /// 轉檔把Excel 資料存到 DB
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Transfer()
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                #region 抓Excel檔案 轉成 model
                // Excel 檔案位置
                DateTime startTime = DateTime.Now;
                string projectFile = Server.MapPath("~/FileUploads");
                string fileName = @"Exhibit 29.xlsx"; //預設
                string configFileName = ConfigurationManager.AppSettings["fileA7Name"];
                if (!string.IsNullOrWhiteSpace(configFileName))
                    fileName = configFileName; //config 設定就取代
                string path = Path.Combine(projectFile, fileName);
                FileStream stream = System.IO.File.Open(path, FileMode.Open, FileAccess.Read);

                string pathType = path.Split('.')[1]; //抓副檔名
                List<Exhibit29Model> dataModel = getExcel(pathType, stream); //Excel轉成 Exhibit10Model

                string proName = "Transfer";
                string tableName = string.Empty;
                #endregion

                #region txtlog 檔案名稱
                string txtpath = "Exhibit29Transfer.txt"; //預設txt名稱
                string configTxtName = ConfigurationManager.AppSettings["txtLogA7Name"];
                if (!string.IsNullOrWhiteSpace(configTxtName))
                    txtpath = configTxtName; //有設定webConfig且不為空就取代
                #endregion

                #region save Moody_Tm_YYYY(A71)
                tableName = "Moody_Tm_YYYY";
                bool flagA71 = saveA71(dataModel); //save to DB
                bool A81Log = saveLog(tableName, fileName, proName, flagA71, startTime, DateTime.Now); //寫sql Log
                TxtLog.txtLog(tableName, flagA71, startTime, txtLocation(txtpath)); //寫txt L7g
                #endregion

                result.RETURN_FLAG = flagA71;
                if (!result.RETURN_FLAG)
                {
                    result.DESCRIPTION = (flagA71 ? string.Empty : "A71 Error! ");
                }
                else
                {
                    result.DESCRIPTION = "Success!";
                }
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = ex.Message;
            }
            return Json(result);
        }

        /// <summary>
        /// 前端抓資料時呼叫
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetData(string type)
        {
            MSGReturnModel result = new MSGReturnModel();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = "No Data!";
            try
            {
                switch (type)
                {
                    case "A71": //Moody_Tm_YYYY(A71)資料
                        if (db.Moody_Tm_YYYY.Count() > 0)
                        {
                            result.RETURN_FLAG = true;
                            result.Datas = Json(db.Moody_Tm_YYYY.ToList());
                        }
                        break;
                    case "A72"://抓Tm_Adjust_YYYY(A72)資料
                        if (db.Moody_Tm_YYYY.Count() > 0)
                        {                         
                            List<object> odatas = new List<object>();
                            DataTable datas = getExhibit29ModelFromDb(db.Moody_Tm_YYYY.ToList());
                            odatas.Add(datas.Columns.Cast<DataColumn>()
                                 .Select(x => x.ColumnName)
                                 .ToArray()); //第一列 由Columns 組成Title 
                            for (var i = 0; i < datas.Rows.Count; i++)
                            {
                                List<string> str = new List<string>();
                                for (int j = 0; j < datas.Rows[i].ItemArray.Count();j ++)
                                {
                                    if (datas.Columns[j].ToString().IndexOf("TM") > -1)
                                    {
                                        str.Add("\"" + datas.Columns[j] + "\":\"" + datas.Rows[i].ItemArray[j].ToString()+ "\"");
                                    }
                                    else
                                    {
                                        str.Add("\"" + datas.Columns[j] + "\":" + datas.Rows[i].ItemArray[j].ToString());
                                    }
                                    //object 格式為 'column' : Rows.Data
                                }
                                odatas.Add(JsonConvert.DeserializeObject<IDictionary<string, object>>
                                    ("{" +string.Join(",",str)+"}")); //第二列以後組成 object
                            }
                            result.Datas = Json(odatas);
                            if(odatas.Count > 2)
                            result.RETURN_FLAG = true;
                        }
                        break;
                    case "A73"://抓GM_YYYY(A73)資料
                        if (db.Moody_Tm_YYYY.Count() > 0)
                        {
                            List<object> odatas = new List<object>();
                            DataTable datas = getExhibit29ModelFromDb(db.Moody_Tm_YYYY.ToList());
                            odatas.Add(datas.Columns.Cast<DataColumn>()
                                 .Where(x=> A73Array.Contains(x.ColumnName))
                                 .Select(x =>  x.ColumnName)
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
                            result.Datas = Json(odatas);
                            if (odatas.Count > 2)
                                result.RETURN_FLAG = true;
                        }
                        break;
                }
                if (result.RETURN_FLAG)
                    result.DESCRIPTION = "Success!";
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = ex.Message;
            }
            return Json(result);
        }

        [HttpPost]
        public JsonResult GetExcel(string type)
        {
            MSGReturnModel result = new MSGReturnModel();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = "請確認檔案是否開啟!";
            try
            {
                switch (type)
                {
                    case "A72":
                        if (db.Moody_Tm_YYYY.Count() > 0)
                        {
                            string path = @"A72.xlsx"; //預設
                            DataTable datas = getExhibit29ModelFromDb(db.Moody_Tm_YYYY.ToList());
                            result.RETURN_FLAG = FileRelated.DataTableToExcel(datas, ExcelLocation(path), string.Empty);
                        }
                        break;
                    case "A73":
                        if (db.Moody_Tm_YYYY.Count() > 0)
                        {
                            string path = @"A73.xlsx"; //預設
                            DataTable datas = getExhibit29ModelFromDb(db.Moody_Tm_YYYY.ToList()); 
                            DataTable newData = new DataTable(); //要組新的 Table                           
                            foreach (var itme in A73Array)
                            {
                                newData.Columns.Add(itme, typeof(string)); //組 column
                            }
                            List<string>[] A73datas = new List<string>[A73Array.Count]; //需求的欄位資料
                            for (int i = 0; i < A73Array.Count; i++)
                            {
                                //取得需求的欄位資料
                                A73datas[i] = datas.AsEnumerable().Select(x => x.Field<string>(A73Array[i])).ToList();
                            }
                            if (A73datas.Count() > 0 && A73datas[0].Count > 0) //有資料
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
                                result.RETURN_FLAG = FileRelated.DataTableToExcel(newData, ExcelLocation(path), string.Empty);
                            }
                            else
                            {
                                result.DESCRIPTION = "No Data!";
                            }
                        }
                        break;
                }
                if (result.RETURN_FLAG)
                    result.DESCRIPTION = "Success !";
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = ex.Message;
            }
            return Json(result);
        }

        #region private function

        #region save Moody_Monthly_PD_Info(A71)
        /// <summary>
        /// Save  Moody_Monthly_PD_Info(A81)
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        private bool saveA71(List<Exhibit29Model> dataModel)
        {
            bool flag = true;
            try
            {
                foreach (var item in db.Moody_Tm_YYYY)
                {
                    db.Moody_Tm_YYYY.Remove(item); //資料全刪除
                }
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
            }
            catch (Exception ex)
            {
                foreach (var item in db.Moody_Monthly_PD_Info)
                {
                    db.Moody_Monthly_PD_Info.Remove(item); //失敗先刪除
                }
                flag = false;
            }
            return flag;
        }
        #endregion

        #region DB Moody_Tm_YYYY 組成 DataTable
        private DataTable getExhibit29ModelFromDb(List<Moody_Tm_YYYY> dbDatas)
        {
            DataTable dt = new DataTable();
            try
            {
                #region 找出錯誤的參數
                //超過的筆對紀錄 => string 開始的參數,List<string>要相加的參數
                Dictionary<string, List<string>> overData =
                    new Dictionary<string, List<string>>();
                string errorKey = string.Empty; //錯誤起始欄位
                string last_FromTo = string.Empty; //上一個 From_To
                double last_value = 0d; //上一個default的參數(#)
                foreach (Moody_Tm_YYYY item in dbDatas) //第一次迴圈先抓出不符合的項目
                {
                    double now_default_value =  //目前的default 參數
                        TypeTransfer.doubleNToDouble(item.Default_Value);
                    if (now_default_value >= last_value) //下一筆比上一筆大(正常情況)
                    {
                        if (!string.IsNullOrWhiteSpace(errorKey)) //假如上一筆是超過的參數
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

                        if (!string.IsNullOrWhiteSpace(errorKey)) //上一個是錯誤的,修改錯誤記錄資料
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
                        //不錯任何動作
                    }
                    else //無錯誤 (columns 加入原本 參數)
                    {
                        if(errorData.Count > 0) //上一筆是錯誤情形
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
                if (errorData.Count > 0) //此為最後一筆為錯誤時觸發
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
                                           select(
                                           (from z in err2
                                           select getDbValueINColume(y, z))
                                           .Sum() )).Sum() / (err2.Count * err.Count));
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
                    else //其他無行並行的只要單獨處理某特例欄位(合併)
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

                #endregion
            }
            catch (Exception ex)
            {
                
            }
            return dt;
        }
        #endregion

        #region 
        private double getDbValueINColume(Moody_Tm_YYYY db,string cname)
        {
            if (cname == Ref.A7_Type.Aaa.ToString())
                 return TypeTransfer.doubleNToDouble(db.Aaa);
            if (cname == Ref.A7_Type.Aa1.ToString())
                return TypeTransfer.doubleNToDouble(db.Aa1);
            if (cname == Ref.A7_Type.Aa2.ToString())
                return TypeTransfer.doubleNToDouble(db.Aa2);
            if (cname == Ref.A7_Type.Aa3.ToString())
                return TypeTransfer.doubleNToDouble(db.Aa3);
            if (cname == Ref.A7_Type.A1.ToString())
                return TypeTransfer.doubleNToDouble(db.A1);
            if (cname == Ref.A7_Type.A2.ToString())
                return TypeTransfer.doubleNToDouble(db.A2);
            if (cname == Ref.A7_Type.A3.ToString())
                return TypeTransfer.doubleNToDouble(db.A3);
            if (cname == Ref.A7_Type.Baa1.ToString())
                return TypeTransfer.doubleNToDouble(db.Baa1);
            if (cname == Ref.A7_Type.Baa2.ToString())
                return TypeTransfer.doubleNToDouble(db.Baa2);
            if (cname == Ref.A7_Type.Baa3.ToString())
                return TypeTransfer.doubleNToDouble(db.Baa3);
            if (cname == Ref.A7_Type.Ba1.ToString())
                return TypeTransfer.doubleNToDouble(db.Ba1);
            if (cname == Ref.A7_Type.Ba2.ToString())
                return TypeTransfer.doubleNToDouble(db.Ba2);
            if (cname == Ref.A7_Type.Ba3.ToString())
                return TypeTransfer.doubleNToDouble(db.Ba3);
            if (cname == Ref.A7_Type.B1.ToString())
                return TypeTransfer.doubleNToDouble(db.B1);
            if (cname == Ref.A7_Type.B2.ToString())
                return TypeTransfer.doubleNToDouble(db.B2);
            if (cname == Ref.A7_Type.B3.ToString())
                return TypeTransfer.doubleNToDouble(db.B3);
            if (cname == Ref.A7_Type.Caa1.ToString())
                return TypeTransfer.doubleNToDouble(db.Caa1);
            if (cname == Ref.A7_Type.Caa2.ToString())
                return TypeTransfer.doubleNToDouble(db.Caa2);
            if (cname == Ref.A7_Type.Caa3.ToString())
                return TypeTransfer.doubleNToDouble(db.Caa3);
            if (cname == Ref.A7_Type.Ca_C.ToString())
                return TypeTransfer.doubleNToDouble(db.Ca_C);
            if (cname == Ref.A7_Type.WR.ToString())
                return TypeTransfer.doubleNToDouble(db.WR);
            if (cname == Ref.A7_Type.Default.ToString())
                return TypeTransfer.doubleNToDouble(db.Default_Value);
            return 0d;
        }
        #endregion

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

        #region get Excel to List<Exhibit29Model>
        /// <summary>
        /// 把Excel 資料轉換成 Exhibit29Model
        /// </summary>
        /// <param name="pathType">string</param>
        /// <param name="stream">Stream</param>
        /// <returns>ExhibitModels</returns>
        private List<Exhibit29Model> getExcel(string pathType, Stream stream)
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

        #endregion private function

    }
}