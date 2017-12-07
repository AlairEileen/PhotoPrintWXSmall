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
    }
    public class GoodsPic
    {
        public List<FileModel<string>> HeaderPics { get; set; }
        public List<FileModel<string>> BodyPics { get; set; }
    }
    public class GoodsModelType
    {
        public string PrintType { get; set; }
        public List<PrintType> PrintTypeList { get; set; }
    }

    public class PrintType
    {
        public string PaperType { get; set; }
        public List<PaperType> PaperTypeList { get; set; }
    }

    public class PaperType
    {
        public string SizeType { get; set; }
        public List<SizeType> SizeTypeList { get; set; }
    }

    public class SizeType
    {

    }
}
