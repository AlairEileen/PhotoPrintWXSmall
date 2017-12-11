using MongoDB.Driver;
using PhotoPrintWXSmall.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    }
}
