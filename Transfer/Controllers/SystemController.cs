using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Transfer.Infrastructure;
using Transfer.Models.Interface;
using Transfer.Models.Repository;
using Transfer.Utility;
using static Transfer.Enum.Ref;

namespace Transfer.Controllers
{
    [Authorize]
    public class SystemController : CommonController
    {
        private ICommon CommonFunction;
        private ISystemRepository SystemRepository;

        public SystemController()
        {
            this.CommonFunction = new Common();
            this.SystemRepository = new SystemRepository();
        }

        /// <summary>
        /// 抓取使用者的 menu 設定
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public JsonResult GetUser(string name)
        {
            MSGReturnModel result = new MSGReturnModel();
            List<CheckBoxListInfo> data
                = SystemRepository.getMenu(name);
            if (data.Any())
            {
                result = new MSGReturnModel()
                {
                    RETURN_FLAG = true,
                    Datas = Json(Extension.CheckBoxString("menuSet", data, null, 4).ToString())
                };
            }
            else
                result = new MSGReturnModel()
                {
                    RETURN_FLAG = false,
                    DESCRIPTION = Message_Type.not_Find_Any.GetDescription()
                };
            return Json(result);
        }

        /// <summary>
        /// 設定主頁 (預留)
        /// </summary>
        /// <returns></returns>
        [UserAuth("Index,System")]
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult SaveMenu(List<CheckBoxListInfo> data, string userName)
        {
            MSGReturnModel result = SystemRepository.saveMenu(data, userName);
            return Json(result);
        }

        /// <summary>
        /// 設定 Account 的權限
        /// </summary>
        /// <returns></returns>
        [UserAuth("SetMenu,System")]
        public ActionResult SetAccount()
        {
            //List<CheckBoxListInfo> data
            //    = SystemRepository.getMenu(AccountController.CurrentUserName);
            //ViewBag.menuCheckbox = data;

            ViewBag.users = new SelectList(
                 SystemRepository.getUser()
                 .Select(x => new { Text = x, Value = x }), "Value", "Text");

            return View();
        }

        /// <summary>
        /// 設定 Menu 的權限
        /// </summary>
        /// <returns></returns>
        [UserAuth("SetMenu,System")]
        public ActionResult SetMenu()
        {
            //List<CheckBoxListInfo> data
            //    = SystemRepository.getMenu(AccountController.CurrentUserName);
            //ViewBag.menuCheckbox = data;

            ViewBag.users = new SelectList(
                 SystemRepository.getUser()
                 .Select(x => new { Text = x, Value = x }), "Value", "Text");

            return View();
        }
    }
}