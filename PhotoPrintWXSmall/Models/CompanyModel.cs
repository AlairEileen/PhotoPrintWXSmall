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
        public QiNiuModel QiNiuModel { get; set; }
    }

    public class QiNiuModel
    {
        public QiNiuDAL.Exerciser exerciser = new QiNiuDAL.Exerciser();
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string Bucket { get; set; }
        public string DoMain { get; set; }
        public void UploadFile(string filePath)
        {
            exerciser.UploadFile(filePath, AccessKey, SecretKey, Bucket);
        }
        public async Task<string> GetFileUrl(string fileName)
        {
            return await exerciser.CreateDownloadUrl(DoMain, fileName);
        }
        public void DeleteFile(string fileName)
        {
            exerciser.DeleteFile(fileName, AccessKey, SecretKey, Bucket);
        }
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
