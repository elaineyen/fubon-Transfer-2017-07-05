using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Transfer.Models;
using Transfer.Utility;

namespace Transfer.Controllers
{
    public class MenuModel
    {
        public List<IFRS9_Menu_Main> menu_Main { get; set; }
        public List<IFRS9_Menu_Sub> menu_Sub { get; set; }
    }

    public class AccountController : Controller
    {
        private IFRS9Entities db = new IFRS9Entities();
        // GET: Account
        public ActionResult Login()
        {
            if (TempData.Count > 0 && TempData["Login"] != null)
                ViewBag.Login = TempData["Login"].ToString();
            else
                ViewBag.Login = string.Empty;
            return View();
        }

        [ChildActionOnly]
        public ActionResult Menu()
        {
            bool hasuser = System.Web.HttpContext.Current.User != null;
            bool isAuthenticated = hasuser && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;

            if (isAuthenticated)
            {
                string user = System.Web.HttpContext.Current.User.Identity.Name;

                List<IFRS9_Menu_Sub> subs = db.IFRS9_Menu_Set.AsEnumerable()
                                     .Where(x => user.Equals(x.User_Name))
                                     .Select(x => x.IFRS9_Menu_Sub)
                                     .OrderBy(x => x.Menu_Id).ToList();
                List<IFRS9_Menu_Main> mains = subs.Select(x => x.IFRS9_Menu_Main).Distinct()
                                                   .OrderBy(x => x.Menu).ToList();
                MenuModel menus = new MenuModel()
                {
                    menu_Main = mains,
                    menu_Sub = subs
                };

                return View(menus);
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        public static string CurrentUserName
        {
            get
            {
                var httpContext = System.Web.HttpContext.Current;
                var identity = httpContext.User.Identity.IsAuthenticated;

                if (identity)
                {
                    return httpContext.User.Identity.Name;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //[AllowAnonymous]
        public ActionResult Logon(string userId, string pwd)
        {
            string path = "";
            string domain = string.Empty;
            bool flag = false;
            //bool flag = LdapAuthentication.isAuthenticatrd(path, domain, userId, pwd);
            //if (userId == "test1" && pwd == "1qaz@WSX")
            //{
            //    this.LoginProcess(userId, false);
            //    flag = true;
            //}
            //else
            //{
            //    ModelState.AddModelError("", "請輸入正確的帳號或密碼!");
            //    flag = false;
            //}
            if (db.IFRS9_User.AsEnumerable().Any(x => userId.Equals(x.User_Name) &&
                pwd.Equals(x.User_Password)))
            {
                this.LoginProcess(userId, false);
                flag = true;
            }
            else
            {
                ModelState.AddModelError("", "請輸入正確的帳號或密碼!");
                flag = false;
            }
            if (flag)
            {
                TempData["Login"] = string.Empty;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["Login"] = "請輸入正確的帳號或密碼!";
                return RedirectToAction("Login", "Account");
            }


        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            //清除所有的 session
            Session.RemoveAll();

            //建立一個同名的 Cookie 來覆蓋原本的 Cookie
            HttpCookie cookie1 = new HttpCookie(FormsAuthentication.FormsCookieName, "");
            cookie1.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie1);

            //建立 ASP.NET 的 Session Cookie 同樣是為了覆蓋
            HttpCookie cookie2 = new HttpCookie("ASP.NET_SessionId", "");
            cookie2.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie2);

            return RedirectToAction("Index", "Home");
        }

        private void LoginProcess(string user, bool isRemeber)
        {
            var now = DateTime.Now;

            var ticket = new FormsAuthenticationTicket(
                version: 1,
                name: user,
                issueDate: now,
                expiration: now.AddMinutes(30),
                isPersistent: isRemeber,
                userData: user,
                cookiePath: FormsAuthentication.FormsCookiePath);

            var encryptedTicket = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
            Response.Cookies.Add(cookie);
        }
    }
}