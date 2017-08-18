using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using Transfer.Models.Interface;
using Transfer.Utility;
using Transfer.ViewModels;
using static Transfer.Enum.Ref;

namespace Transfer.Models.Repository
{
    public class A6Repository : IA6Repository, IDbEvent
    {
        #region 其他
        private Common common = new Common();

        protected IFRS9Entities db
        {
            get;
            private set;
        }

        public A6Repository()
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

        #region get A62 Search Year
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<string> GetA62SearchYear()
        {
            List<string> result = new List<string>();
            if (db.Moody_LGD_Info.Any())
            {
                result.Add("ALL");
                result.AddRange(db.Moody_LGD_Info
                    //.AsEnumerable()
                    .Select(x => x.Data_Year).Distinct().OrderBy(x => x));
            }
            return result;
        }
        #endregion

        #region get Moody_LGD_Info(A62)
        /// <summary>
        /// get Moody_LGD_Info(A62)
        /// </summary>
        /// <returns></returns>
        public Tuple<bool, List<Exhibit7Model>> GetA62(string year)
        {
            if (db.Moody_LGD_Info.Any())
            {
                List<Exhibit7Model> data = (from item in db.Moody_LGD_Info
                                            //.AsEnumerable()
                                            .Where(x=> year.Equals("ALL")
                                            || x.Data_Year.Equals(year))
                                            select new Exhibit7Model() //轉型 Datetime
                                            {
                                                Data_Year = item.Data_Year,
                                                Lien_Position = item.Lien_Position,
                                                Recovery_Rate = item.Recovery_Rate.ToString(),
                                                LGD = item.LGD.ToString()
                                                //Recovery_Rate = string.Format("{0}%",
                                                //        (TypeTransfer.doubleNToDouble(item.Recovery_Rate) * 100).ToString()),
                                                //LGD = string.Format("{0}%",
                                                //        (TypeTransfer.doubleNToDouble(item.LGD) * 100).ToString())
                                            }).ToList();
                if(data.Any())
                return new Tuple<bool, List<Exhibit7Model>>(true, data);
            }
            return new Tuple<bool, List<Exhibit7Model>>(false, new List<Exhibit7Model>());
        }
        #endregion

        #endregion

        #region Save Db

        /// <summary>
        /// save A61
        /// </summary>
        /// <returns></returns>
        public MSGReturnModel saveA61()
        {
            return new MSGReturnModel();
        }

        /// <summary>
        /// save A62
        /// </summary>
        /// <param name="dataModel">Exhibit7Model</param>
        /// <returns></returns>
        public MSGReturnModel saveA62(List<Exhibit7Model> dataModel)
        {
            MSGReturnModel result = new MSGReturnModel();
            if (!dataModel.Any())
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type.parameter_Error
                    .GetDescription(Table_Type.A62.ToString());
                return result;
            }
            string dataYear = dataModel.First().Data_Year;
            if (db.Moody_LGD_Info
                .Any(x => dataYear.Equals(x.Data_Year)))
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type.already_Save
                    .GetDescription(Table_Type.A62.ToString());
                return result;
            }
            #region save Moody_LGD_Info(A62)
            db.Moody_LGD_Info.AddRange(
                dataModel.Select(x=> new Moody_LGD_Info() {
                    Data_Year = x.Data_Year,
                    Lien_Position = x.Lien_Position,
                    Recovery_Rate = TypeTransfer.stringToDoubleN(x.Recovery_Rate),
                    LGD = TypeTransfer.stringToDoubleN(x.LGD)
                }));
            #endregion
            try
            {
                db.SaveChanges();
                result.RETURN_FLAG = true;
                result.DESCRIPTION = Message_Type.save_Success.GetDescription();
            }
            catch (DbUpdateException ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type
                                     .save_Fail.GetDescription(Table_Type.A62.ToString(),
                                     $"message: {ex.Message}" +
                                     $", inner message {ex.InnerException?.InnerException?.Message}");
            }
            return result;
        }
        #endregion

        #region Excel 部分
        /// <summary>
        /// 把Excel 資料轉換成 Exhibit7Model
        /// </summary>
        /// <param name="pathType">string</param>
        /// <param name="stream">Stream</param>
        /// <returns>Exhibit7Model</returns>
        public List<Exhibit7Model> getExcel(string pathType, Stream stream)
        {
            DataSet resultData = new DataSet();
            List<Exhibit7Model> dataModel = new List<Exhibit7Model>();
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

                if (resultData.Tables[0].Rows.Count > 5) //判斷有無資料
                {
                    string Data_Year = resultData.Tables[0].Rows[2][3].ToString();
                    dataModel = (from q in resultData.Tables[0].AsEnumerable()
                                 .Skip(3).Take(9)
                                 select getExhibit7Models(q, Data_Year)).ToList();
                    //skip(4) 為排除Excel 前4行(參數可調)
                    if (dataModel.Count() == 9)
                    {
                        //add Sr. Secured Bond &&  (Recovery_Rate & LGD = (1st Lien Bond + 2nd Lien Bond )/2)
                        dataModel.Add(
                            new Exhibit7Model()
                            {
                                Data_Year = Data_Year,
                                Lien_Position = "Sr. Secured Bond",
                                Recovery_Rate =
                                ((TypeTransfer.stringToDouble(dataModel[3].Recovery_Rate)
                                +
                                TypeTransfer.stringToDouble(dataModel[4].Recovery_Rate))/2).ToString(),
                                LGD =
                                ((TypeTransfer.stringToDouble(dataModel[3].LGD)
                                +
                                TypeTransfer.stringToDouble(dataModel[4].LGD)) / 2).ToString(),
                                //Recovery_Rate =
                                //string.Format("{0}%",
                                //((TypeTransfer.doubleNToDouble( //1st Lien Bond
                                //    TypeTransfer.stringToDoubleNByP(dataModel[3].Recovery_Rate)) 
                                //    +
                                //TypeTransfer.doubleNToDouble( //2nd Lien Bond
                                //    TypeTransfer.stringToDoubleNByP(dataModel[4].Recovery_Rate))) / 2)),
                                //LGD =
                                //string.Format("{0}%",
                                //((TypeTransfer.doubleNToDouble( //1st Lien Bond
                                //    TypeTransfer.stringToDoubleNByP(dataModel[3].LGD))
                                //    +
                                //TypeTransfer.doubleNToDouble( //2nd Lien Bond
                                //    TypeTransfer.stringToDoubleNByP(dataModel[4].LGD))) / 2)),
                            });
                    }
                }
            }
            catch
            { }
            return dataModel;

        }
        #endregion

        #region Private Function

        #region datarow 組成 Exhibit7Model
        /// <summary>
        /// datarow 組成 Exhibit7Model
        /// </summary>
        /// <param name="item">DataRow</param>
        /// <returns>Exhibit7Model</returns>
        private Exhibit7Model getExhibit7Models(DataRow item, string Data_Year)
        {
            return new Exhibit7Model()
            {
                Data_Year = Data_Year,
                Lien_Position = TypeTransfer.objToString(item[0]),
                Recovery_Rate = TypeTransfer.objToString(item[3]),
                LGD = (1 - TypeTransfer.objToDouble(item[3])).ToString()
                //Recovery_Rate = string.Format("{0}%", (Recovery_Rate * 100).ToString()),
                //LGD = string.Format("{0}%", (LGD * 100).ToString())
            };
        }
        #endregion

        #endregion
    }
}