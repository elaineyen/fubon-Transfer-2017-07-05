using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
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
        private ID0Repository D0Repository;
        private ICommon CommonFunction;

        public D0Controller()
        {
            this.D0Repository = new D0Repository();
            this.CommonFunction = new Common();
        }

        // GET: D0
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// D05(套用產品組合代碼-房貸)
        /// </summary>
        /// <returns></returns>
        [UserAuth("D05Mortgage,D0")]
        public ActionResult D05Mortgage()
        {
            List<SelectOption> selectOption = new List<SelectOption>();
            selectOption.Add(new SelectOption() { text = "", value = "" });
            selectOption.AddRange((D0Repository.getGroupProductByDebtType(GroupProductCode.M.GetDescription()).Item2)
                .Select(x => new SelectOption()
                { text = (x.Group_Product_Code + " " + x.Group_Product_Name), value = x.Group_Product_Code }));

            ViewBag.GroupProduct = new SelectList(selectOption, "Value", "Text");

            return View();
        }

        /// <summary>
        /// D05(套用產品組合代碼-債券)
        /// </summary>
        /// <returns></returns>
        [UserAuth("D05Bond,D0")]
        public ActionResult D05Bond()
        {
            List<SelectOption> selectOption = new List<SelectOption>();
            selectOption.Add(new SelectOption() { text = "", value = "" });
            selectOption.AddRange((D0Repository.getGroupProductByDebtType(GroupProductCode.B.GetDescription()).Item2)
                .Select(x => new SelectOption()
                { text = (x.Group_Product_Code + " " + x.Group_Product_Name), value = x.Group_Product_Code }));

            ViewBag.GroupProduct = new SelectList(selectOption, "Value", "Text");

            return View();
        }

        /// <summary>
        /// D01Mortgage(套用流程資訊-房貸)
        /// </summary>
        /// <returns></returns>
        [UserAuth("D01Mortgage,D0")]
        public ActionResult D01Mortgage()
        {
            List<SelectOption> selectOption = new List<SelectOption>();
            selectOption.Add(new SelectOption() { text = "", value = "" });
            selectOption.AddRange((D0Repository.getGroupProductByDebtType(GroupProductCode.M.GetDescription()).Item2)
                .Select(x => new SelectOption()
                { text = (x.Group_Product_Code + " " + x.Group_Product_Name), value = x.Group_Product_Code }));

            ViewBag.GroupProduct = new SelectList(selectOption, "Value", "Text");

            return View();
        }

        /// <summary>
        /// D01Bond(套用流程資訊-債券)
        /// </summary>
        /// <returns></returns>
        [UserAuth("D01Bond,D0")]
        public ActionResult D01Bond()
        {
            List<SelectOption> selectOption = new List<SelectOption>();
            selectOption.Add(new SelectOption() { text = "", value = "" });
            selectOption.AddRange((D0Repository.getGroupProductByDebtType(GroupProductCode.B.GetDescription()).Item2)
                .Select(x => new SelectOption()
                { text = (x.Group_Product_Code + " " + x.Group_Product_Name), value = x.Group_Product_Code }));

            ViewBag.GroupProduct = new SelectList(selectOption, "Value", "Text");

            return View();
        }

        #region Get Data

        /// <summary>
        /// 前端抓資料時呼叫
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetD05AllData(string debtType)
        {
            MSGReturnModel result = new MSGReturnModel();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = "";

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
                var D05Data = D0Repository.getD05All(debtType);
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
        public JsonResult GetD05Data(string debtType, string groupProductCode, string productCode, string processingDate)
        {
            MSGReturnModel result = new MSGReturnModel();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Message_Type.not_Find_Any.GetDescription("D05");

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
                var D05Data = D0Repository.getD05(debtType, groupProductCode, productCode, processingDate);
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
        /// <param name="debtType"></param>
        /// <param name="actionType">動作類型(Add Or Modify)</param>
        /// <param name="groupProductCode">套用產品群代碼</param>
        /// <param name="groupProduct">產品群別說明</param>
        /// <param name="productCode">產品</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SaveD05(string debtType, string actionType, string groupProductCode, string productCode)
        {
            MSGReturnModel result = new MSGReturnModel();

            result.RETURN_FLAG = false;
            result.DESCRIPTION = Message_Type.parameter_Error.GetDescription("D05", result.DESCRIPTION);

            try
            {
                if (GroupProductCode.M.ToString().Equals(debtType))
                {
                    debtType = GroupProductCode.M.GetDescription();
                }
                else if (GroupProductCode.B.ToString().Equals(debtType))
                {
                    debtType = GroupProductCode.B.GetDescription();
                }

                D05ViewModel dataModel = new D05ViewModel();
                dataModel.Group_Product_Code = groupProductCode;
                dataModel.Product_Code = productCode;
                dataModel.Processing_Date = DateTime.Now.ToString("yyyy/MM/dd");

                MSGReturnModel resultSave = D0Repository.saveD05(debtType, actionType, dataModel);

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

        #region Get Data

        /// <summary>
        /// 前端抓資料時呼叫
        /// </summary>
        /// <param name="debtType">1.房貸 4.債券</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetGroupProductByDebtType(string debtType)
        {
            MSGReturnModel result = new MSGReturnModel();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = "";

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
                var returnData = D0Repository.getGroupProductByDebtType(debtType);
                result.RETURN_FLAG = returnData.Item1;
                result.Datas = Json(returnData.Item2);

                if (result.RETURN_FLAG == false)
                {
                    result.DESCRIPTION = Message_Type.not_Find_Any.GetDescription("套用產品群代碼");
                }
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
        /// <param name="dataModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetD01AllData(D01ViewModel dataModel)
        {
            MSGReturnModel result = new MSGReturnModel();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = "";

            if (GroupProductCode.M.ToString().Equals(dataModel.DebtType))
            {
                dataModel.DebtType = GroupProductCode.M.GetDescription();
            }
            else if (GroupProductCode.B.ToString().Equals(dataModel.DebtType))
            {
                dataModel.DebtType = GroupProductCode.B.GetDescription();
            }

            try
            {
                var D01Data = D0Repository.getD01All(dataModel.DebtType);
                result.RETURN_FLAG = D01Data.Item1;
                result.Datas = Json(D01Data.Item2);
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
        /// <param name="dataModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetD01Data(D01ViewModel dataModel)
        {
            MSGReturnModel result = new MSGReturnModel();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Message_Type.not_Find_Any.GetDescription("D01");

            if (GroupProductCode.M.ToString().Equals(dataModel.DebtType))
            {
                dataModel.DebtType = GroupProductCode.M.GetDescription();
            }
            else if (GroupProductCode.B.ToString().Equals(dataModel.DebtType))
            {
                dataModel.DebtType = GroupProductCode.B.GetDescription();
            }

            try
            {
                var D01Data = D0Repository.getD01(dataModel);
                result.RETURN_FLAG = D01Data.Item1;
                result.Datas = Json(D01Data.Item2);
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = ex.Message;
            }

            return Json(result);
        }

        #endregion Get Data

        #region SaveD01

        /// <summary>
        /// 新增、俢改
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SaveD01(D01ViewModel data)
        {
            MSGReturnModel result = new MSGReturnModel();

            result.RETURN_FLAG = false;
            result.DESCRIPTION = Message_Type.parameter_Error.GetDescription("D01", result.DESCRIPTION);

            try
            {
                D01ViewModel dataModel = new D01ViewModel();
                dataModel.DebtType = data.DebtType;
                dataModel.ActionType = data.ActionType;
                dataModel.PRJID = data.PRJID;
                dataModel.FLOWID = data.FLOWID;
                dataModel.Group_Product_Code = data.Group_Product_Code;
                dataModel.Publish_Date = DateTime.Now.ToString("yyyy/MM/dd");
                dataModel.Apply_On_Date = data.Apply_On_Date;
                dataModel.Apply_Off_Date = data.Apply_Off_Date;
                dataModel.Issuer = Transfer.Controllers.AccountController.CurrentUserName;
                dataModel.Memo = data.Memo;

                MSGReturnModel resultSave = D0Repository.saveD01(dataModel);

                result.RETURN_FLAG = resultSave.RETURN_FLAG;
                result.DESCRIPTION = Message_Type.save_Success.GetDescription("D01");

                if (!result.RETURN_FLAG)
                {
                    result.DESCRIPTION = Message_Type.save_Fail.GetDescription("D01", resultSave.DESCRIPTION);
                }
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = ex.Message;
            }

            return Json(result);
        }

        #endregion SaveD01

        #region DeleteD01

        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="prjid">專案名稱</param>
        /// <param name="flowid">流程名稱</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteD01(string prjid, string flowid)
        {
            MSGReturnModel result = new MSGReturnModel();

            result.RETURN_FLAG = false;
            result.DESCRIPTION = Message_Type.parameter_Error.GetDescription("D01", result.DESCRIPTION);

            try
            {
                MSGReturnModel resultDelete = D0Repository.deleteD01(prjid, flowid);

                result.RETURN_FLAG = resultDelete.RETURN_FLAG;
                result.DESCRIPTION = Message_Type.delete_Success.GetDescription("D01");

                if (!result.RETURN_FLAG)
                {
                    result.DESCRIPTION = Message_Type.delete_Fail.GetDescription("D01", resultDelete.DESCRIPTION);
                }
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = ex.Message;
            }

            return Json(result);
        }

        #endregion DeleteD01
    }
}