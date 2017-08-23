using System.Linq;
using System.Web;
using System.Web.Mvc;
using Transfer.Controllers;
using Transfer.Models;

namespace Transfer.Infrastructure
{
    public class UserAuthAttribute : AuthorizeAttribute
    {
        private string _href;

        public UserAuthAttribute(string href)
        {
            _href = href;
            db = new IFRS9Entities();
        }

        protected IFRS9Entities db
        {
            get;
            private set;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            string menu_id = string.Empty;
            var sub = db.IFRS9_Menu_Sub.FirstOrDefault(x => x.Href.Equals(_href));
            if (sub != null)
                menu_id = sub.Menu_Id;
            bool flag = db.IFRS9_Menu_Set.FirstOrDefault(
                    x => x.User_Name.Equals(AccountController.CurrentUserName) &&
                    x.Menu_Id.Equals(menu_id)) != null;
            if (httpContext.Request.IsLocal)
                return flag;
            return true;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult("/Home/Error401");
        }
    }
}