using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Transfer.Models.Interface;
using Transfer.Utility;
using Transfer.ViewModels;

namespace Transfer.Models.Repositiry
{
    public class SystemRepository : ISystemRepository , IDbEvent
    {
        protected IFRS9Entities db
        {
            get;
            private set;
        }

        public SystemRepository()
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

        public List<CheckBoxListInfo> getMenu (string userName)
        {
            //List<CheckBoxListInfo> resultData = new List<CheckBoxListInfo>();
            //List<IFRS9_Menu_Set> sets = db.IFRS9_Menu_Set.AsEnumerable()
            //        .Where(x => userName.Equals(x.User_Name)).ToList();
            //resultData.AddRange(db.IFRS9_Menu_Sub.AsEnumerable()
            //    .Select(x =>
            //    {
            //        return new CheckBoxListInfo()
            //        {
            //            Value = x.Menu_Id,
            //            DisplayText = x.Menu_Detail,
            //            IsChecked = 
            //        };
            //    }));
            return new List<CheckBoxListInfo>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="menuSub"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public bool saveMenu(List<CheckBoxListInfo> menuSub, string userName)
        {
            return false;
        }
    }
}