using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tools.Json;
using Tools.Models;

namespace PhotoPrintWXSmall.Models
{
    public class AccountModel : BaseAccount
    {
        public string OpenID { get; set; }
        public List<OrderLocation> OrderLocations { get; set; }
        /// <summary>
        /// 购物车
        /// </summary>
        public List<Shop> ShoppingCart { get; set; }

        public List<Order> Orders { get; set; }
        public List<FileModel<string[]>> UploadImages { get; set; }

    }

    public class Order
    {
        [BsonId]
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId OrderID { get; set; }
        public string OrderNumber { get; set; }
        public List<Shop> ShopList { get; set; }
        public DateTime CreateTime { get; set; }
        public decimal OrderPrice { get; set; }
        public OrderStatus OrderStatus { get; set; }
    }

    public enum OrderStatus
    {
        /// <summary>
        /// 失效
        /// </summary>
        cancel=-1,
        /// <summary>
        /// 待付款
        /// </summary>
        waitingPay=0,
        /// <summary>
        /// 待发货
        /// </summary>
        waitingSend=1,
        /// <summary>
        /// 待收货
        /// </summary>
        waitingGet=2,
        /// <summary>
        /// 待评价
        /// </summary>
        waitAssess=3,
        /// <summary>
        /// 完成
        /// </summary>
        finish=4
    }

    public class Shop
    {
        [BsonId]
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId ShopID { get; set; }
        public GoodsModel Goods { get; set; }
        public int GoodsCount { get; set; }
        public List<FileModel<string[]>> ShopImages { get; set; }
        [JsonConverter(typeof(Tools.Json.DateConverterEndMinute))]
        [BsonDateTimeOptions(Kind =DateTimeKind.Local)]
        public DateTime CreateTime { get; set; }
    }

    public class OrderLocation
    {
        [BsonId]
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId OrderLocationID { get; set; }
        public string[] ProvinceCityAreaArray { get; set; }
        public string ContactPhone { get; set; }
        public string AdressDetail { get; set; }
        public string ContactName { get; set; }
        public bool IsDefault { get; set; }
    }


}
