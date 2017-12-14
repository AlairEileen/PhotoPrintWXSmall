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
using System.IO.Compression;

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
                if (goodsPic.HeaderPics == null)
                {
                    goodsPicCollection.UpdateOne(x => x.GoodsPicID.Equals(goodsPic.GoodsPicID),
                        Builders<GoodsPic>.Update.Set(x => x.HeaderPics, new List<FileModel<string[]>>()));
                }
                if (goodsPic.BodyPics == null)
                {
                    goodsPicCollection.UpdateOne(x => x.GoodsPicID.Equals(goodsPic.GoodsPicID),
                        Builders<GoodsPic>.Update.Set(x => x.BodyPics, new List<FileModel<string[]>>()));
                }
                ParamsCreate3Img params3Img = new ParamsCreate3Img() { FileName = filename, FileDir = ConstantProperty.GoodsImagesDir };
                params3Img.OnFinish += fileModel =>
                {
                    fileModel.FileID = ObjectId.GenerateNewId();
                    if (picType == 0)
                    {
                        var update = Builders<GoodsPic>.Update.Push(x => x.HeaderPics, fileModel);
                        goodsPicCollection.UpdateOne(x => x.GoodsPicID.Equals(goodsPic.GoodsPicID), update);
                    }
                    else
                    {
                        var update = Builders<GoodsPic>.Update.Push(x => x.BodyPics, fileModel);
                        goodsPicCollection.UpdateOne(x => x.GoodsPicID.Equals(goodsPic.GoodsPicID), update);
                    }
                    mongo.GetMongoCollection<FileModel<string[]>>("FileModel").InsertOne(fileModel);
                };
                //ThreadPool.QueueUserWorkItem(new WaitCallback(ImageTool.Create3Img), params3Img);
                new Thread(new ParameterizedThreadStart(ImageTool.Create3Img)).Start(params3Img);
            }
        }

        /// <summary>
        /// 生成订单文件
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        internal string GetOrderFile(ObjectId orderID)
        {
            var account = mongo.GetMongoCollection<AccountModel>().Find(Builders<AccountModel>.Filter.Eq("Orders.OrderID", orderID)).FirstOrDefault();
            var order = account.Orders.Find(x => x.OrderID.Equals(orderID));

            var savePath = ConstantProperty.BaseDir + ConstantProperty.TempDir + order.OrderNumber;
            Directory.CreateDirectory(savePath);
            var orderInfoFilePath = savePath + "/订单信息.txt";
            var orderInfoFile = File.CreateText(orderInfoFilePath);

            var shopInfo = "";
            var goodsCount = 0;
            order.ShopList.ForEach(x =>
            {
            goodsCount += x.GoodsCount;
            var title = x.Goods.GoodsClass == GoodsClass.OneGoods ? x.Goods.SizeType.TypeName + x.Goods.PrintType.TypeName + x.Goods.PaperType.TypeName : x.Goods.Title;
            var type = x.Goods.GoodsClass == GoodsClass.OneGoods ? "单张" : "套餐";
            var picPath = savePath + $@"/{type}";
            if (!Directory.Exists(picPath))
            {
                Directory.CreateDirectory(picPath);
            }
                foreach (var image in x.ShopImages)
                {
                    var ext=image.FileUrlData[0].Substring(image.FileUrlData[0].LastIndexOf("."));
                    File.Copy(ConstantProperty.BaseDir + image.FileUrlData[0],picPath+"/"+title+ext);
                }

            shopInfo += $@"商品标题：{title}{Environment.NewLine}
商品类型：{type}{Environment.NewLine}
商品数量：{x.GoodsCount}{Environment.NewLine}
商品价格：{x.Goods.GoodsPrice}{Environment.NewLine}
合计：{x.GoodsCount * x.Goods.GoodsPrice}{Environment.NewLine}";
        });

            var orderText = $@"订单收件信息--------------{Environment.NewLine}
收件人：{order.OrderLocation.ContactName}{Environment.NewLine}
收件人联系方式：{order.OrderLocation.ContactPhone}{Environment.NewLine}
收件地址：{order.OrderLocation.ProvinceCityAreaArray[0]}{order.OrderLocation.ProvinceCityAreaArray[1]}{order.OrderLocation.ProvinceCityAreaArray[2]} {order.OrderLocation.AdressDetail}{Environment.NewLine}
订单商品信息--------------{Environment.NewLine}
{shopInfo}{Environment.NewLine}
商品数量总计：{goodsCount}{Environment.NewLine}
商品金额总计：{order.OrderPrice}
";
        orderInfoFile.Write(orderText);
            orderInfoFile.Flush();
            orderInfoFile.Close();

            ZipFile.CreateFromDirectory(savePath, savePath + ".zip");
            return savePath + ".zip";
    }

    internal List<Order> GetAllOrders()
    {
        List<Order> orders = new List<Order>();
        mongo.GetMongoCollection<AccountModel>().Find(Builders<AccountModel>.Filter.Empty).ToList().ForEach(x =>
        {
            if (x.Orders != null)
            {
                orders.AddRange(x.Orders);
            }
        });
        return orders;
    }
}
}
