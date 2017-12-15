using PhotoPrintWXSmall.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace PhotoPrintWXSmall.App_Data
{
    public class GoodsData : BaseData<GoodsModel>
    {
        internal OneGoodsMenu GetGoodsMenu(ObjectId printTypeID, ObjectId paperTypeID, ObjectId sizeTypeID)
        {
            var goodsTypeCollection = mongo.GetMongoCollection<GoodsType>();
            var goodsTypeList = goodsTypeCollection.Find(Builders<GoodsType>.Filter.Empty).ToList();

            var printTypes = goodsTypeList.FindAll(x => x.TypeClass == TypeClass.Print);
            var paperTypes = goodsTypeList.FindAll(x => x.TypeClass == TypeClass.Paper);
            var sizeTypes = goodsTypeList.FindAll(x => x.TypeClass == TypeClass.Size);
            GoodsType selectedPrintType = goodsTypeList.Find(x => x.GoodsTypeID.Equals(printTypeID));
            GoodsType selectedPaperType = goodsTypeList.Find(x => x.GoodsTypeID.Equals(paperTypeID));
            GoodsType selectedSizeType = goodsTypeList.Find(x => x.GoodsTypeID.Equals(sizeTypeID));
            GoodsModel goods = null;
            if (printTypeID.Equals(ObjectId.Empty) && paperTypeID.Equals(ObjectId.Empty) && sizeTypeID.Equals(ObjectId.Empty))
            {
                //GetDefaultSelect(printTypes, paperTypes, sizeTypes, out selectedPaperType, out selectedPrintType, out selectedSizeType);
                GetNoSelectOneGoods(printTypes, paperTypes, sizeTypes);
            }
            else if (printTypeID.Equals(ObjectId.Empty) && paperTypeID.Equals(ObjectId.Empty))
            {
                ///sizeTypeID不为空情况
                ///
                GetOnlySizeType(printTypes, paperTypes, sizeTypes, selectedSizeType);
            }
            else if (paperTypeID.Equals(ObjectId.Empty) && sizeTypeID.Equals(ObjectId.Empty))
            {
                GetOnlyPrintType(printTypes, paperTypes, sizeTypes, selectedPrintType);
            }
            else if (printTypeID.Equals(ObjectId.Empty) && sizeTypeID.Equals(ObjectId.Empty))
            {
                GetOnlyPaperType(printTypes, paperTypes, sizeTypes, selectedPaperType);
            }
            else if (printTypeID.Equals(ObjectId.Empty))
            {
                GetNoPrintType(printTypes, paperTypes, sizeTypes, selectedPaperType, selectedSizeType);
            }
            else if (paperTypeID.Equals(ObjectId.Empty))
            {
                GetNoPaperType(printTypes, paperTypes, sizeTypes, selectedPrintType, selectedSizeType);

            }
            else if (sizeTypeID.Equals(ObjectId.Empty))
            {
                GetNoSizeType(printTypes, paperTypes, sizeTypes, selectedPaperType, selectedPrintType);

            }
            else
            {

                goods = collection.Find(x => x.PrintType.GoodsTypeID.Equals(selectedPrintType.GoodsTypeID) &&
               x.PaperType.GoodsTypeID.Equals(selectedPaperType.GoodsTypeID) &&
               x.SizeType.GoodsTypeID.Equals(selectedSizeType.GoodsTypeID)).FirstOrDefault();
                GetCurrentSelect(printTypes, paperTypes, sizeTypes, selectedPaperType, selectedPrintType, selectedSizeType);
            }

            //GetCurrentSelect(printTypes, paperTypes, sizeTypes, selectedPaperType, selectedPrintType, selectedSizeType);

            OneGoodsMenu goodsMenu = new OneGoodsMenu()
            {
                GoodsID = goods != null ? goods.GoodsID : ObjectId.Empty,
                PaperTypes = paperTypes,
                PrintTypes = printTypes,
                SizeTypes = sizeTypes,
                SelectedPaperTypeID = selectedPaperType != null ? selectedPaperType.GoodsTypeID : ObjectId.Empty,
                SelectedPrintTypeID = selectedPrintType != null ? selectedPrintType.GoodsTypeID : ObjectId.Empty,
                SelectedSizeTypeID = selectedSizeType != null ? selectedSizeType.GoodsTypeID : ObjectId.Empty,
                GoodsPrice = goods != null ? goods.GoodsPrice : 0,
                GoodsOldPrice = goods != null ? goods.GoodsOldPrice : 0,
                GoodsSpread = goods != null ? goods.GoodsOldPrice-goods.GoodsPrice : 0
            };
            return goodsMenu;
        }

        internal GoodsModel GetPlanGoodsInfo(ObjectId goodsID)
        {
            var goods = collection.Find(x => x.GoodsID.Equals(goodsID)).FirstOrDefault();
            goods.GoodsSpread = goods.GoodsOldPrice - goods.GoodsPrice;
            return goods;
        }

        internal List<GoodsModel> GetPlanGoodsList(string planTypeID)
        {
            if (string.IsNullOrEmpty(planTypeID))
            {
                return collection.Find(x => x.GoodsClass == GoodsClass.PlanGoods).ToList();

            }
            var list = collection.Find(x => x.PlanType.GoodsTypeID.Equals(new ObjectId(planTypeID))).ToList();
            list.ForEach(x=> { x.GoodsSpread = x.GoodsOldPrice - x.GoodsPrice; });
            return list;
        }

        internal List<GoodsType> GetAllPlanType()
        {
            return mongo.GetMongoCollection<GoodsType>().Find(x => x.TypeClass == TypeClass.Plan).ToList();
        }

        internal GoodsPic GetGoodsPics(GoodsClass goodsClass)
        {
            var goodsPic = mongo.GetMongoCollection<GoodsPic>().Find(x => x.GoodsClass == goodsClass).FirstOrDefault();
            return goodsPic;
        }

        private void GetNoSizeType(List<GoodsType> printTypes, List<GoodsType> paperTypes, List<GoodsType> sizeTypes, GoodsType selectedPaperType, GoodsType selectedPrintType)
        {
            FilteringType(printTypes, (f, g) =>
             f.Eq(x => x.PrintType.GoodsTypeID, printTypes[g].GoodsTypeID) &
             f.Eq(x => x.PaperType.GoodsTypeID, selectedPaperType.GoodsTypeID));

            FilteringType(paperTypes, (f, g) =>
             f.Eq(x => x.PaperType.GoodsTypeID, paperTypes[g].GoodsTypeID) &
             f.Eq(x => x.PrintType.GoodsTypeID, selectedPrintType.GoodsTypeID));

            FilteringType(sizeTypes, (f, g) =>
              f.Eq(x => x.SizeType.GoodsTypeID, sizeTypes[g].GoodsTypeID) &
              f.Eq(x => x.PrintType.GoodsTypeID, selectedPrintType.GoodsTypeID) &
              f.Eq(x => x.PaperType.GoodsTypeID, selectedPaperType.GoodsTypeID));
        }

        private void GetNoPaperType(List<GoodsType> printTypes, List<GoodsType> paperTypes, List<GoodsType> sizeTypes, GoodsType selectedPrintType, GoodsType selectedSizeType)
        {
            FilteringType(printTypes, (f, g) =>
             f.Eq(x => x.PrintType.GoodsTypeID, printTypes[g].GoodsTypeID) &
             f.Eq(x => x.SizeType.GoodsTypeID, selectedSizeType.GoodsTypeID));

            FilteringType(paperTypes, (f, g) =>
             f.Eq(x => x.PaperType.GoodsTypeID, paperTypes[g].GoodsTypeID) &
             f.Eq(x => x.SizeType.GoodsTypeID, selectedSizeType.GoodsTypeID) &
             f.Eq(x => x.PrintType.GoodsTypeID, selectedPrintType.GoodsTypeID));

            FilteringType(sizeTypes, (f, g) =>
            f.Eq(x => x.SizeType.GoodsTypeID, sizeTypes[g].GoodsTypeID) &
            f.Eq(x => x.PrintType.GoodsTypeID, selectedPrintType.GoodsTypeID));
        }

        private void GetNoPrintType(List<GoodsType> printTypes, List<GoodsType> paperTypes, List<GoodsType> sizeTypes, GoodsType selectedPaperType, GoodsType selectedSizeType)
        {
            FilteringType(printTypes, (f, g) =>
               f.Eq(x => x.PrintType.GoodsTypeID, printTypes[g].GoodsTypeID) &
               f.Eq(x => x.PaperType.GoodsTypeID, selectedPaperType.GoodsTypeID) &
               f.Eq(x => x.SizeType.GoodsTypeID, selectedSizeType.GoodsTypeID));

            FilteringType(paperTypes, (f, g) =>
            f.Eq(x => x.PaperType.GoodsTypeID, paperTypes[g].GoodsTypeID) &
            f.Eq(x => x.SizeType.GoodsTypeID, selectedSizeType.GoodsTypeID));

            FilteringType(sizeTypes, (f, g) =>
            f.Eq(x => x.SizeType.GoodsTypeID, sizeTypes[g].GoodsTypeID) &
            f.Eq(x => x.PaperType.GoodsTypeID, selectedPaperType.GoodsTypeID));
        }

        private void GetOnlyPaperType(List<GoodsType> printTypes, List<GoodsType> paperTypes, List<GoodsType> sizeTypes, GoodsType selectedPaperType)
        {
            FilteringType(printTypes, (f, g) =>
            f.Eq(x => x.PrintType.GoodsTypeID, printTypes[g].GoodsTypeID) &
            f.Eq(x => x.PaperType.GoodsTypeID, selectedPaperType.GoodsTypeID));

            FilteringType(paperTypes, (f, g) =>
            f.Eq(x => x.PaperType.GoodsTypeID, paperTypes[g].GoodsTypeID));

            FilteringType(sizeTypes, (f, g) =>
             f.Eq(x => x.SizeType.GoodsTypeID, sizeTypes[g].GoodsTypeID) &
             f.Eq(x => x.PaperType.GoodsTypeID, selectedPaperType.GoodsTypeID));
        }

        private void GetOnlyPrintType(List<GoodsType> printTypes, List<GoodsType> paperTypes, List<GoodsType> sizeTypes, GoodsType selectedPrintType)
        {
            FilteringType(printTypes, (f, g) =>
            f.Eq(x => x.PrintType.GoodsTypeID, printTypes[g].GoodsTypeID));

            FilteringType(paperTypes, (f, g) =>
             f.Eq(x => x.PaperType.GoodsTypeID, paperTypes[g].GoodsTypeID) &
             f.Eq(x => x.PrintType.GoodsTypeID, selectedPrintType.GoodsTypeID));

            FilteringType(sizeTypes, (f, g) =>
            f.Eq(x => x.SizeType.GoodsTypeID, sizeTypes[g].GoodsTypeID) &
            f.Eq(x => x.PrintType.GoodsTypeID, selectedPrintType.GoodsTypeID));
        }

        private void GetOnlySizeType(List<GoodsType> printTypes, List<GoodsType> paperTypes, List<GoodsType> sizeTypes, GoodsType selectedSizeType)
        {
            FilteringType(printTypes, (f, g) =>
            f.Eq(x => x.SizeType.GoodsTypeID, selectedSizeType.GoodsTypeID) &
            f.Eq(x => x.PrintType.GoodsTypeID, printTypes[g].GoodsTypeID));

            FilteringType(paperTypes, (f, g) =>
            f.Eq(x => x.SizeType.GoodsTypeID, selectedSizeType.GoodsTypeID) &
            f.Eq(x => x.PaperType.GoodsTypeID, paperTypes[g].GoodsTypeID));

            FilteringType(sizeTypes, (f, g) =>
            f.Eq(x => x.SizeType.GoodsTypeID, sizeTypes[g].GoodsTypeID));
        }

        private void FilteringType(List<GoodsType> goodsType, Func<FilterDefinitionBuilder<GoodsModel>, int, FilterDefinition<GoodsModel>> func)
        {
            for (int i = 0; i < goodsType.Count; i++)
            {
                var filter = Builders<GoodsModel>.Filter;

                if (collection.Find(filter.Eq(x => x.GoodsClass, GoodsClass.OneGoods) & func(filter, i)).FirstOrDefault() == null)
                {
                    goodsType[i].HasGoods = false;
                }
                else
                {
                    goodsType[i].HasGoods = true;
                }
            }
        }

        private void GetNoSelectOneGoods(List<GoodsType> printTypes, List<GoodsType> paperTypes, List<GoodsType> sizeTypes)
        {
            FilteringType(printTypes, (f, g) =>
            f.Eq(x => x.PrintType.GoodsTypeID, printTypes[g].GoodsTypeID));

            FilteringType(paperTypes, (f, g) =>
            f.Eq(x => x.PaperType.GoodsTypeID, paperTypes[g].GoodsTypeID));

            FilteringType(sizeTypes, (f, g) =>
            f.Eq(x => x.SizeType.GoodsTypeID, sizeTypes[g].GoodsTypeID));

        }

        private void GetCurrentSelect(List<GoodsType> printTypes, List<GoodsType> paperTypes, List<GoodsType> sizeTypes, GoodsType selectedPaperType, GoodsType selectedPrintType, GoodsType selectedSizeType)
        {
            FilteringType(printTypes, (f, g) =>
            f.Eq(x => x.PrintType.GoodsTypeID, printTypes[g].GoodsTypeID) &
            f.Eq(x => x.PaperType.GoodsTypeID, selectedPaperType.GoodsTypeID) &
            f.Eq(x => x.SizeType.GoodsTypeID, selectedSizeType.GoodsTypeID));

            FilteringType(paperTypes, (f, g) =>
            f.Eq(x => x.PaperType.GoodsTypeID, paperTypes[g].GoodsTypeID) &
            f.Eq(x => x.SizeType.GoodsTypeID, selectedSizeType.GoodsTypeID) &
            f.Eq(x => x.PrintType.GoodsTypeID, selectedPrintType.GoodsTypeID));

            FilteringType(sizeTypes, (f, g) =>
            f.Eq(x => x.SizeType.GoodsTypeID, sizeTypes[g].GoodsTypeID) &
            f.Eq(x => x.PrintType.GoodsTypeID, selectedPrintType.GoodsTypeID) &
            f.Eq(x => x.PaperType.GoodsTypeID, selectedPaperType.GoodsTypeID));
        }


        private FilterDefinition<GoodsModel> GetFilter(List<GoodsType> goodsTypes, GoodsType goodstype, FilterDefinitionBuilder<GoodsModel> f, int g, GoodsModelFilterType gmft)
        {
            switch (gmft)
            {
                case GoodsModelFilterType.printList:
                    return f.Eq(x => x.PrintType.GoodsTypeID, goodsTypes[g].GoodsTypeID);
                case GoodsModelFilterType.print:
                    return f.Eq(x => x.PrintType.GoodsTypeID, goodstype.GoodsTypeID);

                case GoodsModelFilterType.paperList:
                    return f.Eq(x => x.PaperType.GoodsTypeID, goodsTypes[g].GoodsTypeID);

                case GoodsModelFilterType.paper:
                    return f.Eq(x => x.PaperType.GoodsTypeID, goodstype.GoodsTypeID);

                case GoodsModelFilterType.sizeList:
                    return f.Eq(x => x.SizeType.GoodsTypeID, goodsTypes[g].GoodsTypeID);

                case GoodsModelFilterType.size:
                    return f.Eq(x => x.SizeType.GoodsTypeID, goodstype.GoodsTypeID);

                default:
                    return f.Empty;

            }
        }

    }
    enum GoodsModelFilterType
    {
        printList = 0,
        print = 1,
        paperList = 2,
        paper = 3,
        sizeList = 4,
        size = 5
    }
}
