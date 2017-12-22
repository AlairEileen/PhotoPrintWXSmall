using PhotoPrintWXSmall.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Tools.Models;
using Tools.Strings;

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
            shop.GoodsCount = 1;
            //var filesCollection = mongo.GetMongoCollection<FileModel<string[]>>("FileModel");
            for (int i = 0; i < shop.ShopImages.Count; i++)
            {
                var file = account.UploadImages.Find(x =>x!=null&&x.FileID!=null&& x.FileID.Equals(shop.ShopImages[i].FileID));
                shop.ShopImages[i] = file;
            }
            shop.CreateTime = DateTime.Now;
            shop.ShopID = ObjectId.GenerateNewId();
            if (account.ShoppingCart==null)
            {
                collection.UpdateOne(x => x.AccountID.Equals(accountID),
                   Builders<AccountModel>.Update.Set(x => x.ShoppingCart, new List<Shop>() ));
            }
            collection.UpdateOne(x => x.AccountID.Equals(accountID),
                Builders<AccountModel>.Update.Push(x => x.ShoppingCart, shop));
        }

        internal void ChangeShoppingCartGoodsNum(ObjectId accountID, ObjectId shopID, int num)
        {
            var filter = Builders<AccountModel>.Filter;
            var filterSum = filter.Eq(x => x.AccountID, accountID) & filter.Eq("ShoppingCart.ShopID", shopID);
            var update = Builders<AccountModel>.Update.Set("ShoppingCart.$.GoodsCount", num);
            collection.UpdateOne(filterSum, update);
        }

        internal void DelShopInCart(ObjectId accountID, ObjectId shopID)
        {
            var filter = Builders<AccountModel>.Filter;
            var filterSum = filter.Eq(x => x.AccountID, accountID);
            var update = Builders<AccountModel>.Update.Pull("ShoppingCart.$.ShopID", shopID);
            collection.UpdateOne(filterSum, update);
        }

        internal List<Shop> GetShoppingCart(ObjectId accountID)
        {
            var list = collection.Find(x => x.AccountID.Equals(accountID)).FirstOrDefault().ShoppingCart;
            list.Sort((x, y) => -x.CreateTime.CompareTo(y.CreateTime));
            return list;
        }

        internal void PushOrder(ObjectId accountID, List<Shop> shopList)
        {
          
        }
       
        /// <summary>
        /// 创建订单
        /// </summary>
        /// <param name="accountID"></param>
        /// <param name="orderLocationID"></param>
        /// <param name="shopList"></param>
        internal void PushOrder(ObjectId accountID, ObjectId orderLocationID, List<Shop> shopList)
        {
            var account = collection.Find(x => x.AccountID.Equals(accountID)).FirstOrDefault();
            var orderLocation = account.OrderLocations.Find(x=>x.OrderLocationID.Equals(orderLocationID));
            if (orderLocation==null)
            {
                throw new Exception("订单收件地址错误");
            }
            if (account.Orders == null)
            {
                collection.UpdateOne(x => x.AccountID.Equals(accountID),
                    Builders<AccountModel>.Update.Set(x => x.Orders, new List<Order>()));
            }

            decimal orderPrice = 0;
            for (int i = 0; i < shopList.Count; i++)
            {
                shopList[i] = account.ShoppingCart.Find(x => x.ShopID.Equals(shopList[i].ShopID));
                orderPrice = shopList[i].Goods.GoodsPrice * shopList[i].GoodsCount;
            }

            var order = new Order()
            {
                ShopList = shopList,
                OrderID = ObjectId.GenerateNewId(),
                CreateTime = DateTime.Now,
                OrderNumber = new RandomNumber().GetRandom1(),
                OrderPrice = orderPrice,
                OrderStatus = OrderStatus.waitingPay,
                OrderLocation = orderLocation
            };
            collection.UpdateOne(x => x.AccountID.Equals(accountID),
                Builders<AccountModel>.Update.Push(x => x.Orders, order));
        }

        internal void ChangeOrderStatus(ObjectId orderID, OrderStatus orderStatus)
        {
            if (orderStatus==OrderStatus.waitingGet)
            {
                return;
            }
            var filter = Builders<AccountModel>.Filter;
            var filterSum = filter.Eq("Orders.OrderID", orderID);
            var update = Builders<AccountModel>.Update.Set("Orders.$.OrderStatus", orderStatus);
            collection.UpdateOne(filterSum, update);
        }
    }
}
