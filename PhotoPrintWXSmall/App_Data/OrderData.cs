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
        internal void PushShoppingCart(string uniacid, ObjectId accountID, Shop shop)
        {
            var filterAccount = GetModelIDAndUniacIDFilter(accountID, uniacid);
            var account = collection.Find(filterAccount).FirstOrDefault();
            if (account.Orders == null)
            {
                collection.UpdateOne(filterAccount,
                    Builders<AccountModel>.Update.Set(x => x.Orders, new List<Order>()));
            }
            var goodsCollection = mongo.GetMongoCollection<GoodsModel>();
            var goods = goodsCollection.Find(x => x.GoodsID.Equals(shop.Goods.GoodsID) && x.uniacid.Equals(uniacid)).FirstOrDefault();
            if (goods.PicsNum != shop.ShopImages.Count)
            {
                throw new Exception("图片数量与套餐不符合");
            }
            shop.Goods = goods;
            shop.GoodsCount = 1;
            //var filesCollection = mongo.GetMongoCollection<FileModel<string[]>>("FileModel");
            for (int i = 0; i < shop.ShopImages.Count; i++)
            {
                var file = account.UploadImages.Find(x => x != null && x.FileID != null && x.FileID.Equals(shop.ShopImages[i].FileID));
                shop.ShopImages[i] = file;
            }
            shop.CreateTime = DateTime.Now;
            shop.ShopID = ObjectId.GenerateNewId();
            if (account.ShoppingCart == null)
            {
                collection.UpdateOne(filterAccount,
                   Builders<AccountModel>.Update.Set(x => x.ShoppingCart, new List<Shop>()));
            }
            collection.UpdateOne(filterAccount,
                Builders<AccountModel>.Update.Push(x => x.ShoppingCart, shop));
        }

        internal void ChangeShoppingCartGoodsNum(string uniacid, ObjectId accountID, ObjectId shopID, int num)
        {
            var filter = Builders<AccountModel>.Filter;
            var filterSum = GetModelIDAndUniacIDFilter(accountID, uniacid) & filter.Eq("ShoppingCart.ShopID", shopID);
            var update = Builders<AccountModel>.Update.Set("ShoppingCart.$.GoodsCount", num);
            collection.UpdateOne(filterSum, update);
        }

        internal void DelShopInCart(string uniacid, ObjectId accountID, ObjectId shopID)
        {
            var filterSum = GetModelIDAndUniacIDFilter(accountID, uniacid);
            var update = Builders<AccountModel>.Update.Pull("ShoppingCart.$.ShopID", shopID);
            collection.UpdateOne(filterSum, update);
        }

        internal List<Shop> GetShoppingCart(string uniacid, ObjectId accountID)
        {
            var list = collection.Find(GetModelIDAndUniacIDFilter(accountID, uniacid)).FirstOrDefault().ShoppingCart;
            list.Sort((x, y) => -x.CreateTime.CompareTo(y.CreateTime));
            return list;
        }

        /// <summary>
        /// 创建订单
        /// </summary>
        /// <param name="accountID"></param>
        /// <param name="orderLocationID"></param>
        /// <param name="shopList"></param>
        internal Order PushOrder(string uniacid, ObjectId accountID, ObjectId orderLocationID, List<Shop> shopList, out AccountModel account)
        {
            var accountFilter = GetModelIDAndUniacIDFilter(accountID, uniacid);
            account = collection.Find(accountFilter).FirstOrDefault();
            var orderLocation = account.OrderLocations.Find(x => x.OrderLocationID.Equals(orderLocationID));
            if (orderLocation == null)
            {
                throw new Exception("订单收件地址错误");
            }
            if (account.Orders == null)
            {
                collection.UpdateOne(accountFilter,
                    Builders<AccountModel>.Update.Set(x => x.Orders, new List<Order>()));
            }

            decimal orderPrice = 0;
            for (int i = 0; i < shopList.Count; i++)
            {
                shopList[i] = account.ShoppingCart.Find(x => x.ShopID.Equals(shopList[i].ShopID));
                orderPrice = shopList[i].Goods.GoodsPrice * shopList[i].GoodsCount;
            }
            var companyModel = mongo.GetMongoCollection<CompanyModel>().Find(x => x.uniacid.Equals(uniacid)).FirstOrDefault();
            if (companyModel == null || companyModel.OrderProperty == null)
            {
                throw new Exception();
            }
            var order = new Order()
            {
                ShopList = shopList,
                OrderID = ObjectId.GenerateNewId(),
                CreateTime = DateTime.Now,
                OrderNumber = new RandomNumber().GetRandom1(),
                OrderPrice = orderPrice + companyModel.OrderProperty.DefaultCarriage,
                OrderStatus = OrderStatus.waitingPay,
                OrderLocation = orderLocation,
                Carriage = companyModel.OrderProperty.DefaultCarriage

            };
            collection.UpdateOne(accountFilter,
                Builders<AccountModel>.Update.Push(x => x.Orders, order));

            for (int i = 0; i < shopList.Count; i++)
            {
                collection.UpdateOne(accountFilter, Builders<AccountModel>.Update.Pull(x => x.ShoppingCart, shopList[i]));
            }
            return order;
        }

        internal void GetOrderAndAccount(string uniacid, ObjectId accountID, ObjectId orderID, out AccountModel account, out Order order)
        {
            account = GetModelByIDAndUniacID(accountID, uniacid);
            order = account.Orders.Find(x => x.OrderID.Equals(orderID));
        }

        internal List<Order> GetOrderList(string uniacid, ObjectId accountID, int orderStatus)
        {
            var account = GetModelByIDAndUniacID(accountID, uniacid);
            List<Order> orders = null;
            if (account.Orders != null)
            {
                if (orderStatus == -2)
                {
                    orders = account.Orders;
                }
                else
                {
                    orders = account.Orders.FindAll(x => x.OrderStatus == (OrderStatus)orderStatus);
                }
                orders.Sort((x, y) => -x.CreateTime.CompareTo(y.CreateTime));
            }
            return orders;
        }

        internal List<Shop> GetShoppingCartList(string uniacid, ObjectId accountID, List<ObjectId> shopIDList)
        {
            var account = GetModelByIDAndUniacID(accountID, uniacid);
            if (account == null || account.ShoppingCart == null)
            {
                return null;
            }
            List<Shop> shops = new List<Shop>();
            foreach (var item in shopIDList)
            {
                shops.Add(account.ShoppingCart.Find(x => x.ShopID.Equals(item)));
            }
            return shops;
        }

        internal void ChangeOrderStatus(string uniacid, ObjectId orderID, OrderStatus orderStatus)
        {
            if (orderStatus == OrderStatus.waitingGet)
            {
                return;
            }
            var filter = Builders<AccountModel>.Filter;
            var filterSum = filter.Eq("Orders.OrderID", orderID) & filter.Eq(x => x.uniacid, uniacid);
            var update = Builders<AccountModel>.Update.Set("Orders.$.OrderStatus", orderStatus);
            collection.UpdateOne(filterSum, update);
        }


    }
}
