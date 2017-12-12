using PhotoPrintWXSmall.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Tools.Models;

namespace PhotoPrintWXSmall.App_Data
{
    public class OrderData : BaseData<AccountModel>
    {



        internal void PushShoppingCart(ObjectId accountID, Shop shop)
        {
            var account = collection.Find(x => x.AccountID.Equals(accountID)).FirstOrDefault();
            if (account.Orders == null)
            {
                collection.UpdateOne(x => x.AccountID.Equals(accountID),
                    Builders<AccountModel>.Update.Set(x => x.Orders, new List<Order>()));
            }
            var goodsCollection = mongo.GetMongoCollection<GoodsModel>();
            var goods = goodsCollection.Find(x => x.GoodsID.Equals(shop.Goods.GoodsID)).FirstOrDefault();
            if (goods.PicsNum != shop.ShopImages.Count)
            {
                throw new Exception("图片数量与套餐不符合");
            }
            shop.Goods = goods;
            var filesCollection = mongo.GetMongoCollection<FileModel<string[]>>();
            for (int i = 0; i < shop.ShopImages.Count; i++)
            {
                shop.ShopImages[i] = filesCollection.Find(x => x.FileID.Equals(shop.ShopImages[i].FileID)).FirstOrDefault();
            }
            shop.CreateTime = DateTime.Now;
            collection.UpdateOne(x => x.AccountID.Equals(accountID),
                Builders<AccountModel>.Update.Push(x => x.ShoppingCart, shop));
        }
    }
}
