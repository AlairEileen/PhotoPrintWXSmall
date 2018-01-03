﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PhotoPrintWXSmall.Models;
using Tools.ResponseModels;
using MongoDB.Driver;
using Tools.DB;
using Tools;
using Newtonsoft.Json;
using Tools.Json;
using MongoDB.Bson;
using System.IO;
using Tools.Response;
using PhotoPrintWXSmall.App_Data;
using Tools.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PhotoPrintWXSmall.Controllers
{
    public class AccountController : BaseController<AccountData, AccountModel>
    {

        /// <summary>
        /// 请求登录
        /// </summary>
        /// <param name="code"></param>
        /// <param name="iv"></param>
        /// <param name="encryptedData"></param>
        /// <returns></returns>
        [HttpGet]
        public string GetAccountID(string uniacid, string code, string iv, string encryptedData)
        {
            try
            {
                BaseResponseModel<AccountModel> responseModel = new BaseResponseModel<AccountModel>();

                //WXSmallAppCommon.Models.WXAccountInfo wXAccount = WXSmallAppCommon.WXInteractions.WXLoginAction.ProcessRequest(code, iv, encryptedData);
                ///微擎方式
                WXSmallAppCommon.Models.WXAccountInfo wXAccount = We7Tools.We7Tools.GetWeChatUserInfo(uniacid, code, iv, encryptedData);
                var accountCard = thisData.SaveOrdUpdateAccount(uniacid, wXAccount);
                ActionParams stautsCode = ActionParams.code_error;
                if (accountCard != null)
                {
                    responseModel.JsonData = accountCard;
                    stautsCode = ActionParams.code_ok;
                }
                responseModel.StatusCode = stautsCode;
                JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
                string[] param = new string[] { "StatusCode", "JsonData", "AccountID" };
                jsonSerializerSettings.ContractResolver = new LimitPropsContractResolver(param);
                string jsonString = JsonConvert.SerializeObject(responseModel, jsonSerializerSettings);
                return jsonString;
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

        /// <summary>
        /// 获取所有地址列表
        /// </summary>
        /// <param name="accountID">账户ID</param>
        /// <returns></returns>
        public string GetOrderLocations(string uniacid, string accountID)
        {
            try
            {
                List<OrderLocation> ols = thisData.GetOrderLocations(uniacid, new ObjectId(accountID));

                return new BaseResponseModel<List<OrderLocation>>() { StatusCode = ols == null ? ActionParams.code_null : ActionParams.code_ok, JsonData = ols }.ToJson();
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;

                throw;
            }
        }

        /// <summary>
        /// 添加或者修改地址
        /// </summary>
        /// <param name="accountID">账户ID</param>
        /// <returns></returns>
        public string SaveOrderLocation(string uniacid, string accountID)
        {
            try
            {
                string json = new StreamReader(Request.Body).ReadToEnd();
                OrderLocation orderLocation = JsonConvert.DeserializeObject<OrderLocation>(json);
                thisData.SaveOrderLocation(uniacid, new ObjectId(accountID), orderLocation);
                return JsonResponseModel.SuccessJson;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

        /// <summary>
        /// 变更默认地址
        /// </summary>
        /// <param name="accountID">账户ID</param>
        /// <param name="orderLocationID">手机地址ID</param>
        /// <returns></returns>
        public string SetDefaultOrderLocation(string uniacid, string accountID, string orderLocationID)
        {
            try
            {
                thisData.SetDefaultOrderLocation(uniacid, new ObjectId(accountID), new ObjectId(orderLocationID));
                return JsonResponseModel.SuccessJson;
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

        /// <summary>
        /// 获取用户默认收件地址
        /// </summary>
        /// <param name="accountID">账户ID</param>
        /// <returns></returns>
        public string GetDefaultOrderLocation(string uniacid, string accountID)
        {
            try
            {
                OrderLocation orderLocation = thisData.GetDefaultOrderLocation(uniacid, new ObjectId(accountID));
                return new BaseResponseModel<OrderLocation>() { StatusCode = ActionParams.code_ok, JsonData = orderLocation }.ToJson();
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

        /// <summary>
        /// 删除收货地址
        /// </summary>
        /// <param name="accountID"></param>
        /// <param name="orderLocationID"></param>
        /// <returns></returns>
        public string DelOrderLocation(string uniacid, string accountID, string orderLocationID)
        {
            try
            {
                thisData.DelOrderLocation(uniacid, new ObjectId(accountID), new ObjectId(orderLocationID));
                return JsonResponseModel.SuccessJson;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="accountID">账户ID</param>
        /// <param name="fileID">文件ID</param>
        /// <returns></returns>
        public string DelFile(string uniacid, string accountID, string fileID)
        {
            try
            {
                thisData.DelFile(uniacid, new ObjectId(accountID), new ObjectId(fileID));
                return JsonResponseModel.SuccessJson;
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

        /// <summary>
        /// 获取用户所有文件
        /// </summary>
        /// <param name="accountID"></param>
        /// <returns></returns>
        public string GetAllFile(string uniacid, string accountID)
        {
            try
            {
                List<FileModel<string[]>> list = thisData.GetAllFile(uniacid, new ObjectId(accountID));
                return new BaseResponseModel<List<FileModel<string[]>>>() { StatusCode = list == null ? ActionParams.code_null : ActionParams.code_ok, JsonData = list }.ToJson();
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }
    }
}
