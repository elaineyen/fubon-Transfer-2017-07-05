using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Transfer.Controllers;
using Transfer.Models.Interface;
using Transfer.Utility;
using static Transfer.Enum.Ref;

namespace Transfer.Models.Repository
{
    public class SystemRepository : ISystemRepository, IDbEvent
    {
        public SystemRepository()
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

        /// <summary>
        ///
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public List<CheckBoxListInfo> getMenu(string userName)
        {
            List<CheckBoxListInfo> resultData = new List<CheckBoxListInfo>();
            List<string> Menu_Ids = db.IFRS9_Menu_Set.AsEnumerable()
                    .Where(x => userName.Equals(x.User_Name))
                    .Select(x => x.Menu_Id).ToList();
            resultData.AddRange(db.IFRS9_Menu_Sub.AsEnumerable()
                .Where(x => !"System".Equals(x.Menu))
                .Select(x =>
                {
                    //return new CheckBoxListInfo(
                    //    x.Menu_Id,
                    //    x.Menu_Detail,
                    //    Menu_Ids.Contains(x.Menu_Id));
                    return new CheckBoxListInfo()
                    {
                        Value = x.Menu_Id,
                        DisplayText = x.Menu_Detail,
                        IsChecked = Menu_Ids.Contains(x.Menu_Id)
                    };
                }));
            return resultData;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public List<string> getUser()
        {
            string admin = AccountController.CurrentUserName;
            List<string> result = new List<string>() { "" };
            result.AddRange(db.IFRS9_User.AsEnumerable()
                .Where(x => !x.User_Name.Equals(admin))
                .Select(x => x.User_Name).ToList());
            return result;
        }

        public void SaveChange()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="menuSub"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public MSGReturnModel saveMenu(List<CheckBoxListInfo> menuSub, string userName)
        {
            MSGReturnModel result = new MSGReturnModel();
            if (!menuSub.Any() && userName.IsNullOrWhiteSpace())
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type.parameter_Error.GetDescription();
                return result;
            }
            List<IFRS9_Menu_Set> sets = db.IFRS9_Menu_Set
                .Where(x => userName.Equals(x.User_Name)).ToList();
            foreach (CheckBoxListInfo item in menuSub)
            {
                IFRS9_Menu_Set set = sets.FirstOrDefault(x => item.Value.Equals(x.Menu_Id));
                if (set != null) //原本有設定
                {
                    if (!item.IsChecked) //設定無權限
                        db.IFRS9_Menu_Set.Remove(set);
                }
                else //原本無設定
                {
                    if (item.IsChecked)
                        db.IFRS9_Menu_Set.Add(new IFRS9_Menu_Set()
                        {
                            User_Name = userName,
                            Menu_Id = item.Value
                        });
                }
            }
            try
            {
                db.SaveChanges();
                result.RETURN_FLAG = true;
                result.DESCRIPTION = Message_Type.save_Success.GetDescription();
            }
            catch (DbUpdateException ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Message_Type.save_Fail
                    .GetDescription(null,
                    $"message: {ex.Message}" +
                    $", inner message {ex.InnerException?.InnerException?.Message}");
            }
            return result;
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
    }
}