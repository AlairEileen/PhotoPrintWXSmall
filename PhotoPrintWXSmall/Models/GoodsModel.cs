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
    public class GoodsModel
    {
        [BsonId]
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId GoodsID { get; set; }
        public GoodsPic Images { get; set; }
        public GoodsType PaperType { get; set; }
        public GoodsType PrintType { get; set; }
        public GoodsType SizeType { get; set; }
        public GoodsType PlanType { get; set; }
        public decimal GoodsPrice { get; set; }
        public int PicsNum { get; set; }
        public string Title { get; set; }
        public GoodsClass GoodsClass { get; set; }

    }



    public class GoodsPic
    {
        [BsonId]
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId GoodsPicID { get; set; }
        public GoodsClass GoodsClass { get; set; }

        public List<FileModel<string[]>> HeaderPics { get; set; }
        public List<FileModel<string[]>> BodyPics { get; set; }
    }

    public enum GoodsClass
    {
        /// <summary>
        /// 单张照片商品套餐
        /// </summary>
        OneGoods=0,
        /// <summary>
        /// 套餐商品
        /// </summary>
        PlanGoods=1
    }

    public class OneGoodsMenu
    {
        [BsonId]
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId OneGoodsMenuID { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId SelectedPaperTypeID { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId SelectedPrintTypeID { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId SelectedSizeTypeID { get; set; }
        public List<GoodsType> PaperTypes { get; set; }
        public List<GoodsType> PrintTypes { get; set; }
        public List<GoodsType> SizeTypes { get; set; }
        public decimal GoodsPrice { get; set; }
    }

    public class GoodsType
    {
        [BsonId]
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId GoodsTypeID { get; set; }
        public string TypeName { get; set; }
        public TypeClass TypeClass { get; set; }
        /// <summary>
        /// 存在该商品
        /// </summary>
        public bool HasGoods { get; set; }
        public decimal GoodsPrice { get; set; }
    }

    public enum TypeClass
    {
        /// <summary>
        /// 冲印
        /// </summary>
        Print=0,
        /// <summary>
        /// 纸张
        /// </summary>
        Paper =1,
        /// <summary>
        /// 尺寸
        /// </summary>
        Size =2,
        /// <summary>
        /// 套餐类型
        /// </summary>
        Plan = 3
    }
}
