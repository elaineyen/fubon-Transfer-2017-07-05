using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Transfer.Utility;

namespace Transfer.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Login()
        {
            if (TempData.Count > 0 && TempData["Login"] != null)
                ViewBag.Login = TempData["Login"].ToString();
            else
                ViewBag.Login = string.Empty;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //[AllowAnonymous]
        public ActionResult Logon(string userId,string pwd)
        {
            string path = "";
            string domain = string.Empty;
            bool flag = false;
            //bool flag = LdapAuthentication.isAuthenticatrd(path, domain, userId, pwd);
            if (userId == "test1" && pwd == "1qaz@WSX")
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