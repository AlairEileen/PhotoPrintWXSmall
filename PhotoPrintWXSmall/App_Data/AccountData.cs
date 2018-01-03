using MongoDB.Bson;
using MongoDB.Driver;
using PhotoPrintWXSmall.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WXSmallAppCommon.Models;
using Tools.Models;

namespace PhotoPrintWXSmall.App_Data
{
    public class AccountData : BaseData<AccountModel>
    {

        /// <summary>
        /// 调取微信用户，更新或者保存本地用户
        /// </summary>
        /// <param name="wXAccount">微信用户</param>
        /// <returns></returns>
        internal AccountModel SaveOrdUpdateAccount(string uniacid, WXAccountInfo wXAccount)
        {
            Console.WriteLine("在SaveOrdUpdateAccount");
            AccountModel accountCard = null;
            if (wXAccount.OpenId != null)
            {
                var filter = Builders<AccountModel>.Filter.Eq(x => x.OpenID, wXAccount.OpenId) &
                   Builders<AccountModel>.Filter.Eq(x => x.uniacid, uniacid);
                var update = Builders<AccountModel>.Update.Set(x => x.LastChangeTime, DateTime.Now);
                accountCard = collection.FindOneAndUpdate<AccountModel>(filter, update);
                Console.WriteLine($"在SaveOrdUpdateAccount{accountCard == null}");

                if (accountCard == null)
                {
                    //string avatarUrl = DownloadAvatar(wXAccount.AvatarUrl, wXAccount.OpenId);
                    string avatarUrl = wXAccount.AvatarUrl;
                    accountCard = new AccountModel() { uniacid = uniacid, OpenID = wXAccount.OpenId, AccountName = wXAccount.NickName, Gender = wXAccount.GetGender, AccountAvatar = avatarUrl, CreateTime = DateTime.Now, LastChangeTime = DateTime.Now };
                    collection.InsertOne(accountCard);
                }
            }
            return accountCard;
        }



        /// <summary>
        /// 保存或者修改订单地址
        /// </summary>
        /// <param name="accountID">账户ID</param>
        /// <param name="orderLocation">订单地址对象</param>
        internal void SaveOrderLocation(string uniacid, ObjectId accountID, OrderLocation orderLocation)
        {
            UpdateDefinition<AccountModel> update = null;
            FilterDefinition<AccountModel> filter = Builders<AccountModel>.Filter.Eq(x => x.AccountID, accountID) & Builders<AccountModel>.Filter.Eq(x => x.uniacid, uniacid);
            var account = collection.Find(filter).FirstOrDefault();
            ///数据库空收件地址列表处理
            if (account.OrderLocations == null)
            {
                collection.UpdateOne(filter, Builders<AccountModel>.Update.Set(x => x.OrderLocations, new List<OrderLocation>()));
            }
            ///清空默认收件地址
            if (orderLocation.IsDefault)
            {
                foreach (var item in account.OrderLocations)
                {
                    var filterSum = filter
                         & Builders<AccountModel>.Filter.Eq("OrderLocations.OrderLocationID", item.OrderLocationID);
                    update = Builders<AccountModel>.Update.Set("OrderLocations.$.IsDefault", false);
                    collection.UpdateOne(filterSum, update);
                }
            }
            var filterUpdateOrInsert = filter;
            ///增加收件地址
            if (orderLocation.OrderLocationID.Equals(ObjectId.Empty))
            {
                orderLocation.OrderLocationID = ObjectId.GenerateNewId();
                update = Builders<AccountModel>.Update.Push(x => x.OrderLocations, orderLocation);
            }
            ///修改收件地址
            else
            {
                filterUpdateOrInsert = filter
                    & Builders<AccountModel>.Filter
                    .Eq("OrderLocations.OrderLocationID", orderLocation.OrderLocationID);
                update = Builders<AccountModel>.Update.Set("OrderLocations.$", orderLocation);
            }
            ///统一提交
            collection.UpdateOne(filterUpdateOrInsert, update);
        }

        internal List<OrderLocation> GetOrderLocations(string uniacid, ObjectId accountID)
        {
            var account = GetModelByIDAndUniacID(accountID, uniacid);
            if (account == null || account.OrderLocations == null)
            {
                return null;
            }
            return account.OrderLocations;
        }

        internal void SetDefaultOrderLocation(string uniacid, ObjectId accountID, ObjectId orderLocationID)
        {

            FilterDefinition<AccountModel> filter = Builders<AccountModel>.Filter.Eq(x => x.AccountID, accountID)
                & Builders<AccountModel>.Filter.Eq(x => x.uniacid, uniacid);
            UpdateDefinition<AccountModel> update = null;
            var account = collection.Find(x => x.AccountID.Equals(accountID)).FirstOrDefault();
            foreach (var item in account.OrderLocations)
            {
                var filterSum = filter
                     & Builders<AccountModel>.Filter
                 .Eq("OrderLocations.OrderLocationID", item.OrderLocationID);
                update = Builders<AccountModel>.Update.Set("OrderLocations.$.IsDefault", item.OrderLocationID.Equals(orderLocationID) ? true : false);
                collection.UpdateOne(filterSum, update);
            }
        }

        internal OrderLocation GetDefaultOrderLocation(string uniacid, ObjectId accountID)
        {
            var account = GetModelByIDAndUniacID(accountID, uniacid);
            return account.OrderLocations.Find(x => x.IsDefault);
        }

        /// <summary>
        /// 删除订单地址
        /// </summary>
        /// <param name="accountID">账户ID</param>
        /// <param name="orderLocationID">订单地址ID</param>
        internal void DelOrderLocation(string uniacid, ObjectId accountID, ObjectId orderLocationID)
        {
        
            var filter = Builders<AccountModel>.Filter.Eq(x => x.AccountID, accountID)& Builders<AccountModel>.Filter.Eq(x => x.uniacid, uniacid);
            var account = collection.Find(filter).FirstOrDefault();
            var update = Builders<AccountModel>.Update.Pull(x => x.OrderLocations, account.OrderLocations.Find(x => x.OrderLocationID.Equals(orderLocationID)));
            collection.UpdateOne(filter, update);
        }
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="accountID"></param>
        /// <param name="fileID"></param>
        internal void DelFile(string uniacid, ObjectId accountID, ObjectId fileID)
        {
            FilterDefinition<AccountModel> filter = Builders<AccountModel>.Filter.Eq(x => x.AccountID, accountID)
                & Builders<AccountModel>.Filter.Eq(x => x.uniacid, uniacid);
            collection.UpdateOne(filter,
                Builders<AccountModel>.Update.Pull("UploadImages.$.FileID", fileID));
        }
        /// <summary>
        /// 获取用户所有文件
        /// </summary>
        /// <param name="accountID"></param>
        /// <returns></returns>
        internal List<FileModel<string[]>> GetAllFile(string uniacid, ObjectId accountID)
        {
            var account = GetModelByIDAndUniacID(accountID, uniacid);
            if (account==null||account.UploadImages==null)
            {
                return null;
            }
            return account.UploadImages;
        }


    }
}
