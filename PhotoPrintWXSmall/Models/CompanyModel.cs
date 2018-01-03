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
    public class CompanyModel
    {
        [BsonId]
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId CompanyID { get; set; }
        public List<CompanyUser> CompanyUsers { get; set; }
        public OrderProperty OrderProperty { get; set; }
        public string uniacid { get; set; }
        public ProcessMiniInfo ProcessMiniInfo { get; set; }
    }

    public class ProcessMiniInfo
    {
        public string Detail { get; set; }
        public string Name { get; set; }
        public FileModel<string[]> Logo { get; set; }
    }

    public class OrderProperty
    {
        public decimal DefaultCarriage { get; set; }
    }

    public class CompanyUser
    {
        [BsonId]
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId CompanyUserID { get; set; }
        public string CompanyUserName { get; set; }
        public string CompanyUserPassword { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
