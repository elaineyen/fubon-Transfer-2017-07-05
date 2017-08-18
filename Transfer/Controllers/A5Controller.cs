using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Transfer.Infrastructure;
using Transfer.Models.Interface;
using Transfer.Models.Repository;
using Transfer.Utility;
using Transfer.ViewModels;
using static Transfer.Enum.Ref;

namespace Transfer.Controllers
{

    [Authorize]
    public class A5Controller : CommonController
    {
        private ICommon CommonFunction;
        public ICacheProvider Cache { get; set; }
        private List<SelectOption> searchOption = null;
        private List<string> sType = new List<string>() { "購買日評等", "基準日最近評等" };
        public A5Controller()
        {
            //this.A4Repository = new A4Repository();
            this.CommonFunction = new Common();
            //this.Cache = new DefaultCacheProvider();
            searchOption = new List<SelectOption>();
            searchOption.AddRange(EnumUtil.GetValues<A59_SelectType>()
                        .Select(x => new SelectOption()
                        {
                            text = x.GetDescription(),
                            value = x.ToString()
                        }));
        }

        /// <summary>
        /// A41(債券明細檔)
        /// </summary>
        /// <returns></returns>
        [UserAuth("A57Detail,A5")]
        public ActionResult A57Detail()
        {
            ViewBag.searchOption = new SelectList(searchOption, "Value", "Text");
            
            ViewBag.sType = new SelectList(
                sType.Select(x => new { Text = x, Value = x }), "Value", "Text");
            return View();
        }

        /// <summary>
        /// 執行減損計算 (債券)
        /// </summary>
        /// <returns></returns>
        [UserAuth("A58Detail,A5")]
        public ActionResult A58Detail()
        {
            ViewBag.searchOption = new SelectList(searchOption, "Value", "Text");
            ViewBag.sType = new SelectList(
                sType.Select(x => new { Text = x, Value = x }), "Value", "Text");
            return View();
        }



    }
}