using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tools.DB;

namespace PhotoPrintWXSmall.App_Data
{
    public abstract class BaseData<T>
    {
        protected MongoDBTool mongo;
        protected IMongoCollection<T> collection;
        protected BaseData()
        {
            mongo = new MongoDBTool();
            collection = mongo.GetMongoCollection<T>();
        }

        protected BaseData(string collectionName)
        {
            mongo = new MongoDBTool();
            collection = mongo.GetMongoCollection<T>(collectionName);
        }

        protected T GetModelByID(ObjectId objectId)
        {
            return collection.Find(Builders<T>.Filter.Eq("_id", objectId)).FirstOrDefault();
        }

        protected List<T> GetAllModel()
        {
            return collection.Find(Builders<T>.Filter.Empty).ToList();
        }
            
    }
}
