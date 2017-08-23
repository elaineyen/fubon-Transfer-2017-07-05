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

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void SaveChange()
        {
            throw new NotImplementedException();
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

        #region Get Data

        /// <summary>
        /// get D05 data
        /// </summary>
        /// <returns></returns>
        public Tuple<bool, List<D05ViewModel>> getD05(string groupProductCode, string groupProduct, string productCode, string processingDate)
        {
            if (db.Group_Product_Code_Mapping.Any())
            {
                var query = from q in db.Group_Product_Code_Mapping
                            select q;

                if (groupProductCode != "")
                {
                    query = query.Where(x => x.Group_Product_Code.Contains(groupProductCode));
                }

                if (groupProduct != "")
                {
                    query = query.Where(x => x.Group_Product.Contains(groupProduct));
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

        /// <summary>
        /// get D05 all data
        /// </summary>
        /// <returns></returns>
        public Tuple<bool, List<D05ViewModel>> getD05All()
        {
            if (db.Group_Product_Code_Mapping.Any())
            {
                return new Tuple<bool, List<D05ViewModel>>
                (
                    true,
                    (
                        from q in db.Group_Product_Code_Mapping.AsEnumerable().OrderBy(x => x.Group_Product_Code).ThenBy(x => x.Product_Code)
                        select DbToD05ViewModel(q)
                    ).ToList()
                );
            }

            return new Tuple<bool, List<D05ViewModel>>(true, new List<D05ViewModel>());
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
                Group_Product = data.Group_Product,
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
        public MSGReturnModel saveD05(string actionType, D05ViewModel dataModel)
        {
            MSGReturnModel result = new MSGReturnModel();

            try
            {
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
                        Group_Product = dataModel.Group_Product,
                        Product_Code = dataModel.Product_Code,
                        Processing_Date = DateTime.Parse(dataModel.Processing_Date)
                    });
                }
                else if (actionType == "Modify")
                {
                    Group_Product_Code_Mapping oldData = db.Group_Product_Code_Mapping.Find(dataModel.Product_Code);
                    oldData.Group_Product_Code = dataModel.Group_Product_Code;
                    oldData.Group_Product = dataModel.Group_Product;
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
    }
}