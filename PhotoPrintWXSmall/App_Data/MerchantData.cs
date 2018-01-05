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
using PhotoPrintWXSmall.Managers;

namespace PhotoPrintWXSmall.App_Data
{
    public class MerchantData : BaseData<GoodsModel>
    {
        /// <summary>
        /// 保存商品类型
        /// </summary>
        /// <param name="goodsType"></param>
        internal void SaveGoodsType(GoodsType goodsType)
        {
            mongo.GetMongoCollection<GoodsType>().InsertOne(goodsType);
        }

        /// <summary>
        /// 添加单张商品菜单
        /// </summary>
        /// <param name="goodsMenu"></param>
        internal void PushOneGoodsMenu(string uniacid, OneGoodsMenu goodsMenu)
        {
            var collection = mongo.GetMongoCollection<GoodsType>();
            var printType = collection.Find(x => x.GoodsTypeID.Equals(goodsMenu.SelectedPrintTypeID) && x.uniacid.Equals(uniacid)).FirstOrDefault();
            var paperType = collection.Find(x => x.GoodsTypeID.Equals(goodsMenu.SelectedPaperTypeID) && x.uniacid.Equals(uniacid)).FirstOrDefault();
            var sizeType = collection.Find(x => x.GoodsTypeID.Equals(goodsMenu.SelectedSizeTypeID) && x.uniacid.Equals(uniacid)).FirstOrDefault();
            var goodsImages = mongo.GetMongoCollection<GoodsPic>().Find(x => x.GoodsClass == GoodsClass.OneGoods && x.uniacid.Equals(uniacid)).FirstOrDefault();
            this.collection.InsertOne(new GoodsModel()
            {
                GoodsPrice = goodsMenu.GoodsPrice,
                GoodsOldPrice = goodsMenu.GoodsOldPrice,
                Images = goodsImages,
                PaperType = paperType,
                PrintType = printType,
                PicsNum = 1,
                SizeType = sizeType,
                GoodsClass = GoodsClass.OneGoods,
                Title = sizeType.TypeName + printType.TypeName + paperType.TypeName,
                uniacid = uniacid
            });
        }

        /// <summary>
        /// 获取商品类型
        /// </summary>
        /// <param name="typeClass"></param>
        /// <returns></returns>
        internal List<GoodsType> GetGoodsTypes(string uniacid, TypeClass typeClass)
        {
            return mongo.GetMongoCollection<GoodsType>().Find(x => x.TypeClass == typeClass && x.uniacid.Equals(uniacid)).ToList();
        }

        /// <summary>
        /// 是否有该商品
        /// </summary>
        /// <param name="goodsMenu"></param>
        /// <returns></returns>
        internal bool HasGoods(string uniacid, OneGoodsMenu goodsMenu)
        {
            var filter = Builders<GoodsModel>.Filter;
            var filterSum = filter.Eq(x => x.uniacid, uniacid)
                & filter.Eq(x => x.GoodsClass, GoodsClass.OneGoods) &
               filter.Eq(x => x.PrintType.GoodsTypeID, goodsMenu.SelectedPrintTypeID)
                & filter.Eq(x => x.PaperType.GoodsTypeID, goodsMenu.SelectedPaperTypeID)
                & filter.Eq(x => x.SizeType.GoodsTypeID, goodsMenu.SelectedSizeTypeID);
            return collection.Find(filterSum).FirstOrDefault() != null;
        }

        /// <summary>
        /// 添加套餐商品类型
        /// </summary>
        /// <param name="planGoodsType"></param>
        internal void PushPlanGoodsType(string uniacid, string planGoodsType)
        {
            mongo.GetMongoCollection<GoodsType>().InsertOne(new GoodsType() { TypeName = planGoodsType, TypeClass = TypeClass.Plan, uniacid = uniacid });
        }

        /// <summary>
        /// 添加套餐商品
        /// </summary>
        /// <param name="goodsModel"></param>
        internal void PushPlanGoods(GoodsModel goodsModel)
        {
            var goodsType = mongo.GetMongoCollection<GoodsType>().Find(x => x.GoodsTypeID.Equals(goodsModel.PlanType.GoodsTypeID) && x.uniacid.Equals(goodsModel.uniacid)).FirstOrDefault();
            goodsModel.PlanType = goodsType;
            goodsModel.GoodsClass = GoodsClass.PlanGoods;
            var goodsImages = mongo.GetMongoCollection<GoodsPic>().Find(x => x.GoodsClass == GoodsClass.PlanGoods && x.uniacid.Equals(goodsModel.uniacid)).FirstOrDefault();
            goodsModel.Images = goodsImages;
            var file = mongo.GetMongoCollection<FileModel<string[]>>("FileModel").Find(x => x.FileID.Equals(goodsModel.GoodsListPic.FileID)).FirstOrDefault();
            goodsModel.GoodsListPic = file;
            collection.InsertOne(goodsModel);
        }

        /// <summary>
        /// 保存商品图片
        /// </summary>
        /// <param name="goodsType"></param>
        /// <param name="picType"></param>
        /// <param name="files"></param>
        /// <param name="hostingEnvironment"></param>
        internal void SaveGoodsFiles(string uniacid, GoodsClass goodsType, int picType, IFormFileCollection files, IHostingEnvironment hostingEnvironment)
        {
            //return  await Task.Run(() =>
            //  {
            long size = 0;
            foreach (var file in files)
            {
                var filename = ContentDispositionHeaderValue
                                .Parse(file.ContentDisposition)
                                .FileName
                                .Trim('"');
                string saveDir = $@"{ConstantProperty.BaseDir}{ConstantProperty.GoodsImagesDir}{uniacid}/";
                string dbSaveDir = $@"{ConstantProperty.GoodsImagesDir}{uniacid}/";
                if (!Directory.Exists(saveDir))
                {
                    Directory.CreateDirectory(saveDir);
                }
                string exString = filename.Substring(filename.LastIndexOf("."));
                string saveName = Guid.NewGuid().ToString("N");
                filename = $@"{saveDir}{saveName}{exString}";

                FileModel<string[]> fileCard = new FileModel<string[]>();
                using (FileStream fs = System.IO.File.Create(filename))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                    string[] fileUrls = new string[] { $@"{dbSaveDir}{saveName}{exString}" };
                }

                var goodsPicCollection = mongo.GetMongoCollection<GoodsPic>();
                var goodsPic = goodsPicCollection.Find(x => x.GoodsClass == goodsType && x.uniacid.Equals(uniacid)).FirstOrDefault();
                if (goodsPic == null)
                {
                    goodsPic = new GoodsPic() { GoodsClass = goodsType, uniacid = uniacid };

                    goodsPicCollection.InsertOne(goodsPic);
                }
                if (goodsPic.HeaderPics == null)
                {
                    goodsPicCollection.UpdateOne(x => x.GoodsPicID.Equals(goodsPic.GoodsPicID) && x.uniacid.Equals(uniacid),
                        Builders<GoodsPic>.Update.Set(x => x.HeaderPics, new List<FileModel<string[]>>()));
                }
                if (goodsPic.BodyPics == null)
                {
                    goodsPicCollection.UpdateOne(x => x.GoodsPicID.Equals(goodsPic.GoodsPicID) && x.uniacid.Equals(uniacid),
                        Builders<GoodsPic>.Update.Set(x => x.BodyPics, new List<FileModel<string[]>>()));
                }
                ParamsCreate3Img params3Img = new ParamsCreate3Img() { FileName = filename, FileDir = ConstantProperty.GoodsImagesDir + $"{uniacid}/" };
                params3Img.OnFinish += fileModel =>
                {
                    size += file.Length;

                    FileManager.Exerciser(uniacid, null, null).SaveFileModel(fileModel);
                    fileModel.FileID = ObjectId.GenerateNewId();
                    if (picType == 0)
                    {
                        var update = Builders<GoodsPic>.Update.Push(x => x.HeaderPics, fileModel);
                        goodsPicCollection.UpdateOne(x => x.GoodsPicID.Equals(goodsPic.GoodsPicID) && x.uniacid.Equals(uniacid), update);
                    }
                    else
                    {
                        var update = Builders<GoodsPic>.Update.Push(x => x.BodyPics, fileModel);
                        goodsPicCollection.UpdateOne(x => x.GoodsPicID.Equals(goodsPic.GoodsPicID) && x.uniacid.Equals(uniacid), update);
                    }
                    ResetGoodsPics(uniacid, goodsType);

                        //mongo.GetMongoCollection<FileModel<string[]>>("FileModel").InsertOne(fileModel);

                    };
                //ThreadPool.QueueUserWorkItem(new WaitCallback(ImageTool.Create3Img), params3Img);
                new Thread(new ParameterizedThreadStart(ImageTool.Create3Img)).Start(params3Img);
            }
            //return size;
            //});
        }

        internal void ResetGoodsPics(string uniacid, GoodsClass goodsType)
        {
            var gp = mongo.GetMongoCollection<GoodsPic>().Find(x => x.uniacid.Equals(uniacid) && x.GoodsClass == goodsType).FirstOrDefault();
            var filter = Builders<GoodsModel>.Filter;
            var filterSum = filter.Eq(x => x.uniacid, uniacid) & filter.Eq(x => x.GoodsClass, goodsType);
            var update = Builders<GoodsModel>.Update.Set(x => x.Images, gp);
            mongo.GetMongoCollection<GoodsModel>().UpdateMany(filterSum, update);
        }

        internal void DelGoodsFiles(string uniacid, GoodsClass goodsType, int picType)
        {

            var goodsPicCollection = mongo.GetMongoCollection<GoodsPic>();
            var goodsPic = goodsPicCollection.Find(x => x.GoodsClass == goodsType && x.uniacid.Equals(uniacid)).FirstOrDefault();
            if (goodsPic == null)
            {
                throw new Exception();
            }
            UpdateDefinition<GoodsPic> update = null;
            if (picType == 0)
            {
                DelGoodsPics(uniacid, goodsPic.HeaderPics);
                update = Builders<GoodsPic>.Update.Set(x => x.HeaderPics, new List<FileModel<string[]>>());
            }
            else if (picType == 1)
            {
                DelGoodsPics(uniacid, goodsPic.BodyPics);
                update = Builders<GoodsPic>.Update.Set(x => x.BodyPics, new List<FileModel<string[]>>());
            }
            goodsPicCollection.UpdateOne(x => x.GoodsClass == goodsType && x.uniacid.Equals(uniacid), update);
        }

        internal void SendOrder(ObjectId orderID, string company, string number)
        {
            var accountCollection = mongo.GetMongoCollection<AccountModel>();
            var accountModel = accountCollection.Find(Builders<AccountModel>.Filter.Eq("Orders.OrderID", orderID)).FirstOrDefault();
            var logistics = new Logistics() { Company = company, Number = number };
            var update = Builders<AccountModel>.Update.Set("Orders.$.Logistics", logistics).Set("Orders.$.OrderStatus", OrderStatus.waitingGet);
            var filter = Builders<AccountModel>.Filter;
            var filterSum = filter.Eq(x => x.AccountID, accountModel.AccountID) & filter.Eq("Orders.OrderID", orderID);
            accountCollection.UpdateOne(filterSum, update);
        }

        internal void DelGoods(ObjectId goodsID)
        {
            collection.DeleteOne(x => x.GoodsID.Equals(goodsID));
        }

        private void DelGoodsPics(string uniacid, List<FileModel<string[]>> picsList)
        {
            if (picsList == null)
            {
                throw new Exception();
            }
            foreach (var item in picsList)
            {
                FileManager.Exerciser(uniacid, null, item.FileUrlData[0]).DelFile();
                FileManager.Exerciser(uniacid, null, item.FileUrlData[1]).DelFile();
                FileManager.Exerciser(uniacid, null, item.FileUrlData[2]).DelFile();
                //File.Delete(ConstantProperty.BaseDir + item.FileUrlData[0]);
                //File.Delete(ConstantProperty.BaseDir + item.FileUrlData[1]);
                //File.Delete(ConstantProperty.BaseDir + item.FileUrlData[2]);
            }
        }


        internal List<GoodsModel> GetAllGoods(string uniacid, GoodsClass goodsClass)
        {
            return collection.Find(x => x.GoodsClass == goodsClass && x.uniacid.Equals(uniacid)).ToList();
        }

        internal async Task<string> SavePlanGoodsListPic(string uniacid, IFormFile file)
        {
            string resultFileId = "";

            long size = 0;

            var filename = ContentDispositionHeaderValue
                                  .Parse(file.ContentDisposition)
                                  .FileName
                                  .Trim('"');
            string saveDir = $@"{ConstantProperty.BaseDir}{ConstantProperty.GoodsImagesDir}{uniacid}/";
            string dbSaveDir = $@"{ConstantProperty.GoodsImagesDir}{uniacid}/";
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
            return await Task.Run(() =>
            {
                ParamsCreate3Img params3Img = new ParamsCreate3Img() { FileName = filename, FileDir = ConstantProperty.GoodsImagesDir + $"{uniacid}/" };
                params3Img.OnFinish += fileModel =>
                {
                    FileManager.Exerciser(uniacid, null, null).SaveFileModel(fileModel);
                    mongo.GetMongoCollection<FileModel<string[]>>("FileModel").InsertOne(fileModel);
                    resultFileId = fileModel.FileID.ToString();
                };
                ImageTool.Create3Img(params3Img);
                return resultFileId;
            });
        }

        internal void PatchCompanyUser(CompanyUser companyUser, string oldPassword)
        {
            var companyCollection = mongo.GetMongoCollection<CompanyModel>();
            var filter = Builders<CompanyModel>.Filter;
            var filterSum = filter.Eq("CompanyUsers.CompanyUserName", companyUser.CompanyUserName) & filter.Eq("CompanyUsers.CompanyUserPassword", oldPassword);
            var company = companyCollection.Find(filterSum).FirstOrDefault();
            if (company == null)
            {
                throw new Exception("用户名或者密码错误");
            }
            filterSum = filter.Eq(x => x.CompanyID, company.CompanyID) & filter.Eq("CompanyUsers.CompanyUserID", company.CompanyUsers.Find(x => x.CompanyUserName.Equals(companyUser.CompanyUserName) && x.CompanyUserPassword.Equals(oldPassword)).CompanyUserID);
            companyCollection.UpdateOne(filterSum, Builders<CompanyModel>.Update.Set("CompanyUsers.$.CompanyUserPassword", companyUser.CompanyUserPassword));
        }

        /// <summary>
        /// 生成订单文件
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        internal string GetOrderFile(ObjectId orderID)
        {
            var accountCollection = mongo.GetMongoCollection<AccountModel>();
            var account = accountCollection.Find(Builders<AccountModel>.Filter.Eq("Orders.OrderID", orderID)).FirstOrDefault();
            var order = account.Orders.Find(x => x.OrderID.Equals(orderID));
            var filter = Builders<AccountModel>.Filter.Eq(x => x.AccountID, account.AccountID) & Builders<AccountModel>.Filter.Eq("Orders.OrderID", orderID);
            accountCollection.UpdateOne(filter, Builders<AccountModel>.Update.Set("Orders.$.Downloaded", true));
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
                    var ext = image.FileUrlData[0].Substring(image.FileUrlData[0].LastIndexOf("."));
                    File.Copy(ConstantProperty.BaseDir + image.FileUrlData[0], picPath + "/" + title + ext, true);
                }

                shopInfo += $@"商品标题：{title}
商品类型：{type}
商品数量：{x.GoodsCount}
商品价格：{x.Goods.GoodsPrice}
合计：{x.GoodsCount * x.Goods.GoodsPrice}";
            });

            var orderText = $@"订单收件信息--------------
收件人：{order.OrderLocation.ContactName}
收件人联系方式：{order.OrderLocation.ContactPhone}
收件地址：{order.OrderLocation.ProvinceCityAreaArray[0]}{order.OrderLocation.ProvinceCityAreaArray[1]}{order.OrderLocation.ProvinceCityAreaArray[2]} {order.OrderLocation.AdressDetail}
{Environment.NewLine}
订单商品信息--------------
{shopInfo}{Environment.NewLine}
合计--------------
商品数量总计：{goodsCount}
商品金额总计：{order.OrderPrice}
订单号码：{order.OrderNumber}";
            orderInfoFile.Write(orderText);
            orderInfoFile.Flush();
            orderInfoFile.Close();
            if (File.Exists(savePath + ".zip"))
            {
                File.Delete(savePath + ".zip");
            }
            ZipFile.CreateFromDirectory(savePath, savePath + ".zip");
            return savePath + ".zip";
        }

        /// <summary>
        /// 获取所有订单信息
        /// </summary>
        /// <returns></returns>
        internal List<Order> GetAllOrders(string uniacid, OrderStatus orderStatus, int downloaded)
        {
            List<Order> orders = new List<Order>();
            var accountModels = mongo.GetMongoCollection<AccountModel>().Find(Builders<AccountModel>.Filter.Eq(x => x.uniacid, uniacid)).ToList();
            accountModels.ForEach(x =>
            {
                if (x.Orders != null)
                {
                    if (orderStatus == OrderStatus.all)
                    {
                        orders.AddRange(x.Orders);
                    }
                    else
                    {
                        foreach (var order in x.Orders)
                        {
                            if (order.OrderStatus == orderStatus && (downloaded == -1 ? true : (order.Downloaded == (downloaded == 1))))
                            {
                                orders.Add(order);
                            }
                        }
                    }
                }

            });
            return orders;
        }

    }
}
