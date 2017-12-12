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
