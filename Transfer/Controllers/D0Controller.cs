using System;
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
    public class D0Controller : CommonController
    {
        private ICommon CommonFunction;
        private ID0Repository D0Repository;

        public D0Controller()
        {
            this.D0Repository = new D0Repository();
            this.CommonFunction = new Common();
        }

        /// <summary>
        /// D05(套用產品組合代碼)
        /// </summary>
        /// <returns></returns>
        [UserAuth("D05,D0")]
        public ActionResult D05()
        {
            return View();
        }

        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="productCode">產品</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteD05(string productCode)
        {
            MSGReturnModel result = new MSGReturnModel();

            result.RETURN_FLAG = false;
            result.DESCRIPTION = Message_Type.parameter_Error.GetDescription("D05", result.DESCRIPTION);

            try
            {
                MSGReturnModel resultDelete = D0Repository.deleteD05(productCode);

                result.RETURN_FLAG = resultDelete.RETURN_FLAG;
                result.DESCRIPTION = Message_Type.delete_Success.GetDescription("D05");

                if (!result.RETURN_FLAG)
                {
                    result.DESCRIPTION = Message_Type.delete_Fail.GetDescription("D05", resultDelete.DESCRIPTION);
                }
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = ex.Message;
            }

            return Json(result);
        }

        // GET: D0
        public ActionResult Index()
        {
            return View();
        }

        #region Get Data

        /// <summary>
        /// 前端抓資料時呼叫
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetD05AllData()
        {
            MSGReturnModel result = new MSGReturnModel();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = "";

            try
            {
                var D05Data = D0Repository.getD05All();
                result.RETURN_FLAG = D05Data.Item1;
                result.Datas = Json(D05Data.Item2);
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = ex.Message;
            }

            return Json(result);
        }

        /// <summary>
        /// 前端抓資料時呼叫
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetD05Data(string groupProductCode, string groupProduct, string productCode, string processingDate)
        {
            MSGReturnModel result = new MSGReturnModel();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Message_Type.not_Find_Any.GetDescription("D05");

            try
            {
                var D05Data = D0Repository.getD05(groupProductCode, groupProduct, productCode, processingDate);
                result.RETURN_FLAG = D05Data.Item1;
                result.Datas = Json(D05Data.Item2);
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = ex.Message;
            }

            return Json(result);
        }

        #endregion Get Data

        /// <summary>
        /// 新增、俢改
        /// </summary>
        /// <param name="actionType">動作類型(Add Or Modify)</param>
        /// <param name="groupProductCode">套用產品群代碼</param>
        /// <param name="groupProduct">產品群別說明</param>
        /// <param name="productCode">產品</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SaveD05(string actionType, string groupProductCode, string groupProduct, string productCode)
        {
            MSGReturnModel result = new MSGReturnModel();

            result.RETURN_FLAG = false;
            result.DESCRIPTION = Message_Type.parameter_Error.GetDescription("D05", result.DESCRIPTION);

            try
            {
                D05ViewModel dataModel = new D05ViewModel();
                dataModel.Group_Product_Code = groupProductCode;
                dataModel.Group_Product = groupProduct;
                dataModel.Product_Code = productCode;
                dataModel.Processing_Date = DateTime.Now.ToString("yyyy/MM/dd");

                MSGReturnModel resultSave = D0Repository.saveD05(actionType, dataModel);

                result.RETURN_FLAG = resultSave.RETURN_FLAG;
                result.DESCRIPTION = Message_Type.save_Success.GetDescription("D05");

                if (!result.RETURN_FLAG)
                {
                    result.DESCRIPTION = Message_Type.save_Fail.GetDescription("D05", resultSave.DESCRIPTION);
                }
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = ex.Message;
            }

            return Json(result);
        }
    }
}