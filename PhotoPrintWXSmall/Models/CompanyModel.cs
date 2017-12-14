using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tools.Json;

namespace PhotoPrintWXSmall.Models
{
    public class CompanyModel
    {
        [BsonId]
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId CompanyID { get; set; }
        public List<CompanyUser> CompanyUsers { get; set; }
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
