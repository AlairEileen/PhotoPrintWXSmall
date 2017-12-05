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
            int stautsCode = (int)(ActionParams.code_error);
            if (accountCard != null)
            {
                responseModel.JsonData = accountCard;
                stautsCode = (int)(ActionParams.code_ok);
            }
            responseModel.StatusCode = stautsCode;
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();

            string[] param = new string[] { "StatusCode", "JsonData", "AccountID" };


            jsonSerializerSettings.ContractResolver = new LimitPropsContractResolver(param);
            string jsonString = JsonConvert.SerializeObject(responseModel, jsonSerializerSettings);
            Console.WriteLine("json#####UserInfo:" + jsonString);
            return jsonString;
        }
    }
}
