using System;
using System.Web.Mvc;
using Transfer.Infrastructure;
using Transfer.Models.Interface;
using Transfer.Models.Repository;
using Transfer.Utility;
using static Transfer.Enum.Ref;

namespace Transfer.Controllers
{
    [Authorize]
    public class C0Controller : CommonController
    {
        private C0Repository C0Repository;
        private ICommon CommonFunction;

        public C0Controller()
        {
            this.C0Repository = new C0Repository();
            this.CommonFunction = new Common();
        }

        // GET: C0
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// C07(減損計算輸出資料-房貸)
        /// </summary>
        /// <returns></returns>
        [UserAuth("C07Mortgage,C0")]
        public ActionResult C07Mortgage()
        {
            return View();
        }

        /// <summary>
        /// C07(減損計算輸出資料-債券)
        /// </summary>
        /// <returns></returns>
        [UserAuth("C07Bond,C0")]
        public ActionResult C07Bond()
        {
            return View();
        }

        #region Get C07
        /// <summary>
        /// 前端抓資料時呼叫
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetC07Data(string debtType, string productCode, string reportDate, string version)
        {
            MSGReturnModel result = new MSGReturnModel();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Message_Type.not_Find_Any.GetDescription("C07");

            if (GroupProductCode.M.ToString().Equals(debtType))
            {
                debtType = GroupProductCode.M.GetDescription();
            }
            else if (GroupProductCode.B.ToString().Equals(debtType))
            {
                debtType = GroupProductCode.B.GetDescription();
            }

            try
            {
                var C07Data = C0Repository.getC07(debtType, productCode, reportDate, version);
                result.RETURN_FLAG = C07Data.Item1;
                result.Datas = Json(C07Data.Item2);
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = ex.Message;
            }

            return Json(result);
        }
        #endregion
    }
}