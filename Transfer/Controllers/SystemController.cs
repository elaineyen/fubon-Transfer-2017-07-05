using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web.Mvc;
using Transfer.Infrastructure;
using Transfer.Models;
using Transfer.Models.Interface;
using Transfer.Models.Repositiry;
using Transfer.Utility;
using Transfer.ViewModels;
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
        /// 設定主頁 (預留)
        /// </summary>
        /// <returns></returns>
        [UserAuth("Index,System")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 設定 Menu 的權限
        /// </summary>
        /// <returns></returns>
        [UserAuth("SetMenu,System")]
        public ActionResult SetMenu()
        {
            return View();
        }

    }
}