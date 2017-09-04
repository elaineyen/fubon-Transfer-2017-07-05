using System;
using System.Collections.Generic;
using System.Linq;
using Transfer.Models.Interface;
using Transfer.Utility;
using Transfer.ViewModels;

namespace Transfer.Models.Repository
{
    public class C0Repository : IC0Repository, IDbEvent
    {
        #region 其他
        protected IFRS9Entities db
        {
            get;
            private set;
        }

        public C0Repository()
        {
            this.db = new IFRS9Entities();
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
        #endregion

        #region Get C07 Data
        /// <summary>
        /// get C07 data
        /// </summary>
        /// <returns></returns>
        public Tuple<bool, List<C07ViewModel>> getC07(string debtType, string productCode, string reportDate, string version)
        {
            if (db.EL_Data_Out.Any())
            {
                var query = from q in db.EL_Data_Out.Where(x => x.Group_Product_Code_Mapping.Group_Product_Code.StartsWith(debtType))
                            select q;

                if (productCode != "")
                {
                    query = query.Where(x => x.Product_Code.Contains(productCode));
                }

                if (reportDate != "")
                {
                    query = query.Where(x => x.Report_Date == reportDate);
                }

                if (version != "")
                {
                    query = query.Where(x => x.Version.ToString().Contains(version));
                }

                return new Tuple<bool, List<C07ViewModel>>((query.Count() > 0 ? true : false),
                    query.AsEnumerable().OrderBy(x => x.Report_Date).ThenBy(x => x.Product_Code).ThenBy(x => x.Reference_Nbr)
                    .Select(x => { return DbToC07ViewModel(x); }).ToList());
            }

            return new Tuple<bool, List<C07ViewModel>>(false, new List<C07ViewModel>());
        }
        #endregion

        #region Db 組成 C07ViewModel
        /// <summary>
        /// Db 組成 C07ViewModel
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private C07ViewModel DbToC07ViewModel(EL_Data_Out data)
        {
            return new C07ViewModel()
            {
                Report_Date = data.Report_Date,
                Processing_Date = data.Processing_Date,
                Product_Code = data.Product_Code,
                Reference_Nbr = data.Reference_Nbr,
                PD = TypeTransfer.doubleNToString(data.PD),
                Lifetime_EL = TypeTransfer.doubleNToString(data.Lifetime_EL),
                Y1_EL = TypeTransfer.doubleNToString(data.Y1_EL),
                EL = TypeTransfer.doubleNToString(data.EL),
                Impairment_Stage = data.Impairment_Stage,
                Version = data.Version.ToString(),
                PRJID = data.PRJID,
                FLOWID = data.FLOWID
            };
        }
        #endregion

    }
}