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

    }
}
