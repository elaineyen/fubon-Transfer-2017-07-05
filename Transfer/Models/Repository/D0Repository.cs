using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Transfer.Models.Interface;
using Transfer.Utility;
using Transfer.ViewModels;
using static Transfer.Enum.Ref;

namespace Transfer.Models.Repository
{
    public class D0Repository : ID0Repository, IDbEvent
    {
        #region 其他

        public D0Repository()
        {
            this.db = new IFRS9Entities();
        }

        protected IFRS9Entities db
        {
            get;
            private set;
        }

        public void SaveChange()
        {
            throw new NotImplementedException();
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

        #endregion 其他

        #region Get GroupProduct By DebtType

        /// <summary>
        /// get Db Group_Product data by debtType
        /// </summary>
        /// <param name="debtType">1.房貸 4.債券</param>
        /// <returns></returns>
        public Tuple<bool, List<GroupProductViewModel>> getGroupProductByDebtType(string debtType)
        {
            if (db.Group_Product.Any())
            {
                var query = from q in db.Group_Product
                                      .Where(x => x.Group_Product_Code.StartsWith(debtType))
                            select q;

                return new Tuple<bool, List<GroupProductViewModel>>((query.Count() > 0 ? true : false),
                    query.AsEnumerable().OrderBy(x => x.Group_Product_Code)
                    .Select(x => { return DbToGroupProductViewModel(x); }).ToList());
            }

            return new Tuple<bool, List<GroupProductViewModel>>(true, new List<GroupProductViewModel>());
        }

        #endregion Get GroupProduct By DebtType

        #region Db 組成 GroupProductViewModel

        /// <summary>
        /// Db 組成 GroupProductViewModel
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private GroupProductViewModel DbToGroupProductViewModel(Group_Product data)
        {
            return new GroupProductViewModel()
            {
                Group_Product_Code = data.Group_Product_Code,
                Group_Product_Name = data.Group_Product_Name
            };
        }

        #endregion Db 組成 GroupProductViewModel

        #region Get Data

        /// <summary>
        /// get D05 all data
        /// </summary>
        /// <returns></returns>
        public Tuple<bool, List<D05ViewModel>> getD05All(string debtType)
        {
            if (db.Group_Product_Code_Mapping.Any())
            {
                return new Tuple<bool, List<D05ViewModel>>
                (
                    true,
                    (
                        from q in db.Group_Product_Code_Mapping.Where(x => x.Group_Product_Code.StartsWith(debtType))
                                                               .AsEnumerable()
                                                               .OrderBy(x => x.Group_Product_Code).ThenBy(x => x.Product_Code)
                        select DbToD05ViewModel(q)
                    ).ToList()
                );
            }

            return new Tuple<bool, List<D05ViewModel>>(true, new List<D05ViewModel>());
        }

        /// <summary>
        /// get D05 data
        /// </summary>
        /// <returns></returns>
        public Tuple<bool, List<D05ViewModel>> getD05(string debtType, string groupProductCode, string productCode, string processingDate)
        {
            if (db.Group_Product_Code_Mapping.Any())
            {
                var query = from q in db.Group_Product_Code_Mapping.Where(x => x.Group_Product_Code.StartsWith(debtType))
                            select q;

                if (groupProductCode != "")
                {
                    query = query.Where(x => x.Group_Product_Code.Contains(groupProductCode));
                }

                if (productCode != "")
                {
                    query = query.Where(x => x.Product_Code.Contains(productCode));
                }

                if (processingDate != "")
                {
                    DateTime dProcessDate = DateTime.Parse(processingDate);
                    query = query.Where(x => x.Processing_Date == dProcessDate);
                }

                return new Tuple<bool, List<D05ViewModel>>((query.Count() > 0 ? true : false),
                    query.AsEnumerable().OrderBy(x => x.Group_Product_Code).ThenBy(x => x.Product_Code).Select(x => { return DbToD05ViewModel(x); }).ToList());
            }

            return new Tuple<bool, List<D05ViewModel>>(false, new List<D05ViewModel>());
        }

        #endregion Get Data

        #region Db 組成 D05ViewModel

        /// <summary>
        /// Db 組成 D05ViewModel
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private D05ViewModel DbToD05ViewModel(Group_Product_Code_Mapping data)
        {
            return new D05ViewModel()
            {
                Group_Product_Code = data.Group_Product_Code,
                Group_Product = data.Group_Product.Group_Product_Name,
                Product_Code = data.Product_Code,
                Processing_Date = TypeTransfer.dateTimeNToString(data.Processing_Date)
            };
        }

        #endregion Db 組成 D05ViewModel

        #region Save D05

        /// <summary>
        /// D05 save db
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        public MSGReturnModel saveD05(string debtType, string actionType, D05ViewModel dataModel)
        {
            MSGReturnModel result = new MSGReturnModel();

            try
            {
                if (debtType != dataModel.Group_Product_Code.Substring(0, 1))
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = "套用產品群代碼 第一個字必須是 " + debtType;
                    return result;
                }

                if (actionType == "Add")
                {
                    if (db.Group_Product_Code_Mapping.Where(x => x.Product_Code == dataModel.Product_Code).Count() > 0)
                    {
                        result.RETURN_FLAG = false;
                        result.DESCRIPTION = "資料重複：" + dataModel.Product_Code + " 已存在";
                        return result;
                    }

                    db.Group_Product_Code_Mapping.Add(
                    new Group_Product_Code_Mapping()
                    {
                        Group_Product_Code = dataModel.Group_Product_Code,
                        Product_Code = dataModel.Product_Code,
                        Processing_Date = DateTime.Parse(dataModel.Processing_Date)
                    });
                }
                else if (actionType == "Modify")
                {
                    Group_Product_Code_Mapping oldData = db.Group_Product_Code_Mapping.Find(dataModel.Product_Code);
                    oldData.Group_Product_Code = dataModel.Group_Product_Code;
                    oldData.Product_Code = dataModel.Product_Code;
                    oldData.Processing_Date = DateTime.Parse(dataModel.Processing_Date);
                }

                db.SaveChanges(); //Save
                result.RETURN_FLAG = true;
            }
            catch (DbUpdateException ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type
                                     .save_Fail.GetDescription("D05",
                                     $"message: {ex.Message}" +
                                     $", inner message {ex.InnerException?.InnerException?.Message}");
            }

            return result;
        }

        #endregion Save D05

        #region Delete D05

        /// <summary>
        /// D05 delete db
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        public MSGReturnModel deleteD05(string proproductCode)
        {
            MSGReturnModel result = new MSGReturnModel();

            try
            {
                var query = db.Group_Product_Code_Mapping.AsEnumerable().Where(x => x.Product_Code == proproductCode);
                db.Group_Product_Code_Mapping.RemoveRange(query);

                db.SaveChanges(); //Save
                result.RETURN_FLAG = true;
            }
            catch (DbUpdateException ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = ex.Message;
            }

            return result;
        }

        #endregion Delete D05

        #region Get Data

        /// <summary>
        /// get D01 all data
        /// </summary>
        /// <param name="debtType">1.房貸 4.債券</param>
        /// <returns></returns>
        public Tuple<bool, List<D01ViewModel>> getD01All(string debtType)
        {
            if (db.Flow_Info.Any())
            {
                return new Tuple<bool, List<D01ViewModel>>
                (
                    true,
                    (
                        from q in db.Flow_Info.Where(x => x.Group_Product_Code.StartsWith(debtType)).AsEnumerable().OrderBy(x => x.PRJID).ThenBy(x => x.FLOWID)
                        select DbToD01ViewModel(q)
                    ).ToList()
                );
            }

            return new Tuple<bool, List<D01ViewModel>>(true, new List<D01ViewModel>());
        }

        /// <summary>
        /// get D01 data
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        public Tuple<bool, List<D01ViewModel>> getD01(D01ViewModel dataModel)
        {
            if (db.Flow_Info.Any())
            {
                var query = from q in db.Flow_Info.Where(x => x.Group_Product_Code.StartsWith(dataModel.DebtType))
                            select q;

                if (dataModel.PRJID.IsNullOrEmpty() == false)
                {
                    query = query.Where(x => x.PRJID.Contains(dataModel.PRJID));
                }

                if (dataModel.FLOWID.IsNullOrEmpty() == false)
                {
                    query = query.Where(x => x.FLOWID.Contains(dataModel.FLOWID));
                }

                if (dataModel.Group_Product_Code.IsNullOrEmpty() == false)
                {
                    query = query.Where(x => x.Group_Product_Code.Contains(dataModel.Group_Product_Code));
                }

                DateTime tempDate;
                if (dataModel.Publish_Date.IsNullOrEmpty() == false)
                {
                    tempDate = DateTime.Parse(dataModel.Publish_Date);
                    query = query.Where(x => x.Publish_Date == tempDate);
                }

                return new Tuple<bool, List<D01ViewModel>>((query.Count() > 0 ? true : false),
                    query.AsEnumerable().OrderBy(x => x.PRJID).ThenBy(x => x.FLOWID).Select(x => { return DbToD01ViewModel(x); }).ToList());
            }

            return new Tuple<bool, List<D01ViewModel>>(false, new List<D01ViewModel>());
        }

        #endregion Get Data

        #region Db 組成 D01ViewModel

        /// <summary>
        /// Db 組成 D01ViewModel
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private D01ViewModel DbToD01ViewModel(Flow_Info data)
        {
            return new D01ViewModel()
            {
                PRJID = data.PRJID,
                FLOWID = data.FLOWID,
                Group_Product_Code = data.Group_Product_Code,
                Publish_Date = TypeTransfer.dateTimeNToString(data.Publish_Date),
                Apply_On_Date = TypeTransfer.dateTimeNToString(data.Apply_On_Date),
                Apply_Off_Date = TypeTransfer.dateTimeNToString(data.Apply_Off_Date),
                Issuer = data.Issuer,
                Memo = data.Memo
            };
        }

        #endregion Db 組成 D01ViewModel

        #region Save D01

        /// <summary>
        /// D01 save db
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        public MSGReturnModel saveD01(D01ViewModel dataModel)
        {
            MSGReturnModel result = new MSGReturnModel();

            try
            {
                if (dataModel.ActionType == "Add")
                {
                    if (db.Flow_Info.Where(x => x.PRJID == dataModel.PRJID && x.FLOWID == dataModel.FLOWID).Count() > 0)
                    {
                        result.RETURN_FLAG = false;
                        result.DESCRIPTION = "資料重複：" + dataModel.PRJID + " " + dataModel.FLOWID + " 已存在";
                        return result;
                    }

                    db.Flow_Info.Add(
                    new Flow_Info()
                    {
                        PRJID = dataModel.PRJID,
                        FLOWID = dataModel.FLOWID,
                        Group_Product_Code = dataModel.Group_Product_Code,
                        Publish_Date = DateTime.Parse(dataModel.Publish_Date),
                        Apply_On_Date = DateTime.Parse(dataModel.Apply_On_Date),
                        Apply_Off_Date = DateTime.Parse(dataModel.Apply_Off_Date),
                        Issuer = dataModel.Issuer,
                        Memo = dataModel.Memo
                    });
                }
                else if (dataModel.ActionType == "Modify")
                {
                    Flow_Info oldData = db.Flow_Info.Where(x => x.PRJID == dataModel.PRJID && x.FLOWID == dataModel.FLOWID).FirstOrDefault();
                    oldData.PRJID = dataModel.PRJID;
                    oldData.FLOWID = dataModel.FLOWID;
                    oldData.Group_Product_Code = dataModel.Group_Product_Code;
                    oldData.Publish_Date = DateTime.Parse(dataModel.Publish_Date);
                    oldData.Apply_On_Date = DateTime.Parse(dataModel.Apply_On_Date);
                    oldData.Apply_Off_Date = DateTime.Parse(dataModel.Apply_Off_Date);
                    oldData.Issuer = dataModel.Issuer;
                    oldData.Memo = dataModel.Memo;
                }

                db.SaveChanges(); //Save
                result.RETURN_FLAG = true;
            }
            catch (DbUpdateException ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type
                                     .save_Fail.GetDescription("D01",
                                     $"message: {ex.Message}" +
                                     $", inner message {ex.InnerException?.InnerException?.Message}");
            }

            return result;
        }

        #endregion Save D01

        #region Delete D01

        /// <summary>
        /// D01 delete db
        /// </summary>
        /// <param name="prjid">專案名稱</param>
        /// <param name="flowid">流程名稱</param>
        /// <returns></returns>
        public MSGReturnModel deleteD01(string prjid, string flowid)
        {
            MSGReturnModel result = new MSGReturnModel();

            try
            {
                var query = db.Flow_Info.AsEnumerable().Where(x => x.PRJID == prjid && x.FLOWID == flowid);
                db.Flow_Info.RemoveRange(query);

                db.SaveChanges(); //Save
                result.RETURN_FLAG = true;
            }
            catch (DbUpdateException ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = ex.Message;
            }

            return result;
        }

        #endregion Delete D01
    }
}