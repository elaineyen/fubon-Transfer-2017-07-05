using System.Web.Mvc;
using Transfer.Infrastructure;
using Transfer.Models.Interface;
using Transfer.Models.Repository;
using Transfer.Utility;

namespace Transfer.Controllers
{
    [Authorize]
    public class C4Controller : CommonController
    {
        private IC4Repository C4Repository;
        private ICommon CommonFunction;

        public C4Controller()
        {
            this.C4Repository = new C4Repository();
            this.CommonFunction = new Common();
            this.Cache = new DefaultCacheProvider();
        }

        public ICacheProvider Cache { get; set; }

        [UserAuth("Index,C4")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Transfer()
        {
            MSGReturnModel result = new MSGReturnModel();
            return Json(result);
        }
    }
}