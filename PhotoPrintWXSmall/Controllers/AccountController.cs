using System;
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
        public string GetAccountID(string code, string iv, string encryptedData)
        {
            try
            {
                BaseResponseModel<AccountModel> responseModel = new BaseResponseModel<AccountModel>();
                WXSmallAppCommon.Models.WXAccountInfo wXAccount = WXSmallAppCommon.WXInteractions.WXLoginAction.ProcessRequest(code, iv, encryptedData);
                var accountCard = thisData.SaveOrdUpdateAccount(wXAccount);
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
        public string GetOrderLocations(string accountID)
        {
            try
            {
                var account = thisData.GetAccount(accountID);
                if (account.OrderLocations == null || account.OrderLocations.Count == 0)
                {
                    return new BaseResponseModel<string>() { StatusCode = ActionParams.code_null }.ToJson();
                }
                return new BaseResponseModel<List<OrderLocation>>() { StatusCode = ActionParams.code_ok, JsonData = account.OrderLocations }.ToJson();
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
        public string SaveOrderLocation(string accountID)
        {
            try
            {
                string json = new StreamReader(Request.Body).ReadToEnd();
                OrderLocation orderLocation = JsonConvert.DeserializeObject<OrderLocation>(json);
                thisData.SaveOrderLocation(accountID, orderLocation);
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
        /// 删除收货地址
        /// </summary>
        /// <param name="accountID"></param>
        /// <param name="orderLocationID"></param>
        /// <returns></returns>
        public string DelOrderLocation(string accountID, string orderLocationID)
        {
            try
            {
                thisData.DelOrderLocation(accountID, orderLocationID);
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
        public string DelFile(string accountID, string fileID)
        {
            try
            {
                thisData.DelFile(new ObjectId(accountID), new ObjectId(fileID));
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
        public string GetAllFile(string accountID)
        {
            try
            {
                List<FileModel<string[]>> list = thisData.GetAllFile(new ObjectId(accountID));
                return new BaseResponseModel<List<FileModel<string[]>>>() { StatusCode = ActionParams.code_ok, JsonData = list }.ToJson();
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }
    }
}
