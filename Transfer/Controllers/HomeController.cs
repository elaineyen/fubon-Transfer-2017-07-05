using System.Web.Mvc;

namespace Transfer.Controllers
{
    public class HomeController : CommonController
    {
        public ActionResult Index()
        {
            ViewBag.manu = "HomeMain";
            return View();
        }
    }
}