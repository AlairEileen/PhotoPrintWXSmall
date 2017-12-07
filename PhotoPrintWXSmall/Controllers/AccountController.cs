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

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PhotoPrintWXSmall.Controllers
{
    public class AccountController : Controller
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
            BaseResponseModel<AccountModel> responseModel = new BaseResponseModel<AccountModel>();

            AccountModel accountCard = null;
            WXSmallAppCommon.Models.WXAccountInfo wXAccount = WXSmallAppCommon.WXInteractions.WXLoginAction.ProcessRequest(code, iv, encryptedData);
            if (wXAccount.OpenId != null)
            {
                var filter = Builders<AccountModel>.Filter.And(Builders<AccountModel>.Filter.Eq(x => x.OpenID, wXAccount.OpenId));
                var collection = new MongoDBTool().GetMongoCollection<AccountModel>();
                var update = Builders<AccountModel>.Update.Set(x => x.LastChangeTime, DateTime.Now);
                accountCard = collection.FindOneAndUpdate<AccountModel>(filter, update);

                if (accountCard == null)
                {
                    //string avatarUrl = DownloadAvatar(wXAccount.AvatarUrl, wXAccount.OpenId);
                    string avatarUrl = wXAccount.AvatarUrl;
                    accountCard = new AccountModel() { OpenID = wXAccount.OpenId, AccountName = wXAccount.NickName, Gender = wXAccount.GetGender, AccountAvatar = avatarUrl, CreateTime = DateTime.Now, LastChangeTime = DateTime.Now };
                    collection.InsertOne(accountCard);
                }
            }
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
            Console.WriteLine("json#####UserInfo:" + jsonString);
            return jsonString;
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
                var account = new MongoDBTool().GetMongoCollection<AccountModel>().Find(x => x.AccountID.Equals(new ObjectId(accountID))).FirstOrDefault();
                if (account.OrderLocations == null || account.OrderLocations.Count == 0)
                {
                    return new BaseResponseModel<string>() { StatusCode = ActionParams.code_null }.ToJson();
                }
                return new BaseResponseModel<List<OrderLocation>>() { StatusCode = ActionParams.code_ok, JsonData = account.OrderLocations }.ToJson();
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson();

                throw;
            }
        }

        //public string SaveOrderLocation(string orderLocationID,string Province,string City,string Area,string ContactPhone,string AdressDetail,string ContactName,bool IsDefault)
        //{

        //}
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
                var collection = new MongoDBTool().GetMongoCollection<AccountModel>();
                UpdateDefinition<AccountModel> update = null;
                FilterDefinition<AccountModel> filter = null;
                ///数据库空收件地址列表处理
                if (collection.Find(x => x.AccountID.Equals(new ObjectId(accountID))).FirstOrDefault().OrderLocations == null)
                {
                    collection.UpdateOne(x => x.AccountID.Equals(new ObjectId(accountID)), Builders<AccountModel>.Update.Set(x => x.OrderLocations, new List<OrderLocation>()));
                }
                ///清空默认收件地址
                if (orderLocation.IsDefault)
                {
                    filter = Builders<AccountModel>.Filter.Eq(x => x.AccountID, new ObjectId(accountID))&
                        Builders<AccountModel>.Filter.Eq("OrderLocations.IsDefault", true);
                    update = Builders<AccountModel>.Update.Set("OrderLocations.$.IsDefault", false);
                    collection.UpdateOne(filter, update);
                }
                ///增加收件地址
                if (orderLocation.OrderLocationID.Equals(ObjectId.Empty))
                {
                    orderLocation.OrderLocationID = ObjectId.GenerateNewId();
                    update = Builders<AccountModel>.Update.Push(x => x.OrderLocations, orderLocation);
                    filter = Builders<AccountModel>.Filter.Eq(x => x.AccountID, new ObjectId(accountID));
                }
                ///修改收件地址
                else
                {
                    filter = Builders<AccountModel>.Filter.Eq(x => x.AccountID, new ObjectId(accountID))
                        & Builders<AccountModel>.Filter
                        .Eq("OrderLocations.OrderLocationID", orderLocation.OrderLocationID);
                    update = Builders<AccountModel>.Update.Set("OrderLocations.$", orderLocation);
                }
                ///统一提交
                collection.UpdateOne(filter, update);
                return JsonResponseModel.SuccessJson();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                
                    
                return JsonResponseModel.ErrorJson();


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
                var filter = Builders<AccountModel>.Filter.Eq(x => x.AccountID, new ObjectId(accountID));
                var update = Builders<AccountModel>.Update.Pull("OrderLocations.$.OrderLocationID", new ObjectId(orderLocationID));
                new MongoDBTool().GetMongoCollection<AccountModel>().UpdateOne(filter, update);
                return JsonResponseModel.SuccessJson();
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson();
                throw;
            }
        }
    }
}
