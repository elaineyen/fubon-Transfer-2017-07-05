using System.Web.Mvc;

namespace Transfer.Controllers
{
    [Authorize]
    public class HomeController : CommonController
    {
        public ActionResult Index()
        {
            ViewBag.manu = "HomeMain";
            return View();
        }

        public ActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }

        public ActionResult Error401()
        {
            ViewBag.status = "401 無此操作權限";
            return View("~/Views/Shared/Error.cshtml");
        }

        public ActionResult Error404()
        {
            ViewBag.status = "404 找不到此頁面";
            return View("~/Views/Shared/Error.cshtml");
        }

        public ActionResult Error403()
        {
            ViewBag.status = "403 禁止: 拒絕存取";
            return View("~/Views/Shared/Error.cshtml");
        }

        public ActionResult Error500()
        {
            ViewBag.status = "500 伺服器錯誤";
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}