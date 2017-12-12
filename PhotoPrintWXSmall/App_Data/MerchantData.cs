using MongoDB.Driver;
using PhotoPrintWXSmall.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting.Internal;
using System.Net.Http.Headers;
using Tools;
using System.IO;
using Tools.Models;
using System.Threading;
using MongoDB.Bson;
using Microsoft.AspNetCore.Hosting;

namespace PhotoPrintWXSmall.App_Data
{
    public class MerchantData : BaseData<GoodsModel>
    {
        internal void SaveGoodsType(GoodsType goodsType)
        {
            mongo.GetMongoCollection<GoodsType>().InsertOne(goodsType);
        }

        internal void PushOneGoodsMenu(OneGoodsMenu goodsMenu)
        {
            var collection = mongo.GetMongoCollection<GoodsType>();
            var printType = collection.Find(x => x.GoodsTypeID.Equals(goodsMenu.SelectedPrintTypeID)).FirstOrDefault();
            var paperType = collection.Find(x => x.GoodsTypeID.Equals(goodsMenu.SelectedPaperTypeID)).FirstOrDefault();
            var sizeType = collection.Find(x => x.GoodsTypeID.Equals(goodsMenu.SelectedSizeTypeID)).FirstOrDefault();
            var goodsImages = mongo.GetMongoCollection<GoodsPic>().Find(x => x.GoodsClass == GoodsClass.OneGoods).FirstOrDefault();
            this.collection.InsertOne(new GoodsModel()
            {
                GoodsPrice = goodsMenu.GoodsPrice,
                Images = goodsImages,
                PaperType = paperType,
                PrintType = printType,
                PicsNum = 1,
                SizeType = sizeType,
                GoodsClass = GoodsClass.OneGoods
            });
        }

        internal List<GoodsType> GetGoodsTypes(TypeClass typeClass)
        {
            return mongo.GetMongoCollection<GoodsType>().Find(x => x.TypeClass == typeClass).ToList();
        }

        internal bool HasGoods(OneGoodsMenu goodsMenu)
        {
            var filter = Builders<GoodsModel>.Filter;
            var filterSum = filter.Eq(x => x.GoodsClass, GoodsClass.OneGoods) &
               filter.Eq(x => x.PrintType.GoodsTypeID, goodsMenu.SelectedPrintTypeID)
                & filter.Eq(x => x.PaperType.GoodsTypeID, goodsMenu.SelectedPaperTypeID)
                & filter.Eq(x => x.SizeType.GoodsTypeID, goodsMenu.SelectedSizeTypeID);
            return collection.Find(filterSum).FirstOrDefault() != null;
        }

        internal void PushPlanGoodsType(string planGoodsType)
        {
            mongo.GetMongoCollection<GoodsType>().InsertOne(new GoodsType() { TypeName = planGoodsType, TypeClass = TypeClass.Plan });
        }

        internal void PushPlanGoods(GoodsModel goodsModel)
        {
            var goodsType = mongo.GetMongoCollection<GoodsType>().Find(x => x.GoodsTypeID.Equals(goodsModel.PlanType.GoodsTypeID)).FirstOrDefault();
            goodsModel.PlanType = goodsType;
            goodsModel.GoodsClass = GoodsClass.PlanGoods;
            collection.InsertOne(goodsModel);
        }

        internal void SaveGoodsFiles(int goodsType, int picType, IFormFileCollection files, IHostingEnvironment hostingEnvironment)
        {
            long size = 0;
            foreach (var file in files)
            {
                var filename = ContentDispositionHeaderValue
                                .Parse(file.ContentDisposition)
                                .FileName
                                .Trim('"');
                string saveDir = $@"{ConstantProperty.BaseDir}{ConstantProperty.GoodsImagesDir}";
                string dbSaveDir = $@"{ConstantProperty.GoodsImagesDir}";
                if (!Directory.Exists(saveDir))
                {
                    Directory.CreateDirectory(saveDir);
                }
                string exString = filename.Substring(filename.LastIndexOf("."));
                string saveName = Guid.NewGuid().ToString("N");
                filename = $@"{saveDir}{saveName}{exString}";

                size += file.Length;
                FileModel<string[]> fileCard = new FileModel<string[]>();
                using (FileStream fs = System.IO.File.Create(filename))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                    string[] fileUrls = new string[] { $@"{dbSaveDir}{saveName}{exString}" };
                }
              
                var goodsPicCollection = mongo.GetMongoCollection<GoodsPic>();
                var goodsPic = goodsPicCollection.Find(x => x.GoodsClass == (GoodsClass)goodsType).FirstOrDefault();
                if (goodsPic == null)
                {
                    goodsPic = new GoodsPic() { GoodsClass = (GoodsClass)goodsType };

                    goodsPicCollection.InsertOne(goodsPic);
                }
                ParamsCreate3Img params3Img = new ParamsCreate3Img() { FileName = filename, FileDir = ConstantProperty.GoodsImagesDir };
                params3Img.OnFinish += fileModel =>
                {
                    fileModel.FileID = ObjectId.GenerateNewId();
                    if (picType == 0)
                    {
                        if (goodsPic.HeaderPics == null)
                        {
                            goodsPicCollection.UpdateOne(x=>x.GoodsPicID.Equals(goodsPic.GoodsPicID),
                                Builders<GoodsPic>.Update.Set(x=>x.HeaderPics,new List<FileModel<string[]>>()));
                        }
                        var update = Builders<GoodsPic>.Update.Push(x=>x.HeaderPics,fileModel);
                        goodsPicCollection.UpdateOne(x=>x.GoodsPicID.Equals(goodsPic.GoodsPicID),update);
                    }
                    else
                    {
                        if (goodsPic.BodyPics == null)
                        {
                            goodsPicCollection.UpdateOne(x => x.GoodsPicID.Equals(goodsPic.GoodsPicID),
                                Builders<GoodsPic>.Update.Set(x => x.BodyPics, new List<FileModel<string[]>>()));
                        }
                        var update = Builders<GoodsPic>.Update.Push(x => x.BodyPics, fileModel);
                        goodsPicCollection.UpdateOne(x => x.GoodsPicID.Equals(goodsPic.GoodsPicID), update);
                    }
                    mongo.GetMongoCollection<FileModel<string[]>>("FileModel").InsertOne(fileModel);
                };
                //ThreadPool.QueueUserWorkItem(new WaitCallback(ImageTool.Create3Img), params3Img);
                new Thread(new ParameterizedThreadStart(ImageTool.Create3Img)).Start(params3Img);
            }
        }
    }
}
