using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using PhotoPrintWXSmall.App_Data;
using PhotoPrintWXSmall.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tools.Response;
using Tools.ResponseModels;
using We7Tools.Extend;

namespace PhotoPrintWXSmall.Controllers
{
    public class GoodsController : BaseController<GoodsData, GoodsModel>
    {
        /// <summary>
        /// 获取单张商品套餐与价格，价格为零时代表套餐未选择
        /// </summary>
        /// <param name="selectedPrintTypeID">当前选中的冲印类型ID</param>
        /// <param name="selectedPaperTypeID">当前选中的纸张ID</param>
        /// <param name="selectedSizeTypeID">当前选中的尺寸ID</param>
        /// <returns></returns>
        public string GetGoodsOne(string uniacid, string selectedPrintTypeID, string selectedPaperTypeID, string selectedSizeTypeID)
        {
            try
            {
                OneGoodsMenu goodsMenu = thisData.GetGoodsMenu(uniacid,
                     string.IsNullOrEmpty(selectedPrintTypeID) ? ObjectId.Empty : new ObjectId(selectedPrintTypeID),
                    string.IsNullOrEmpty(selectedPaperTypeID) ? ObjectId.Empty : new ObjectId(selectedPaperTypeID),
                      string.IsNullOrEmpty(selectedSizeTypeID) ? ObjectId.Empty : new ObjectId(selectedSizeTypeID));
                return new BaseResponseModel<OneGoodsMenu>() { StatusCode = Tools.ActionParams.code_ok, JsonData = goodsMenu }.ToJson();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

        /// <summary>
        /// 获取商品相关图片（HeaderPics：轮播图，BodyPics：详情图片；图片数组0、1、2：大、中、小图）
        /// </summary>
        /// <param name="goodsClass">商品类别（0：单张，1：套餐）</param>
        /// <returns></returns>
        public string GetGoodsPics(string uniacid, GoodsClass goodsClass)
        {
            try
            {
                if (string.IsNullOrEmpty(uniacid))
                {
                    uniacid = HttpContext.Session.GetUniacID();
                }
                GoodsPic goodsPic = thisData.GetGoodsPics(uniacid, goodsClass);
                return new BaseResponseModel<GoodsPic>() { StatusCode = Tools.ActionParams.code_ok, JsonData = goodsPic }.ToJson();
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

        /// <summary>
        /// 获取套餐商品列表
        /// </summary>
        /// <param name="planTypeID">套餐类型ID</param>
        /// <returns></returns>
        public string GetPlanGoodsList(string uniacid, string planTypeID)
        {
            try
            {
                List<GoodsModel> goodsList = thisData.GetPlanGoodsList(uniacid, planTypeID);
                return new BaseResponseModel<List<GoodsModel>>() { StatusCode = Tools.ActionParams.code_ok, JsonData = goodsList }.ToJson();
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }
       
        /// <summary>
        /// 获取套餐商品详情
        /// </summary>
        /// <param name="goodsID">商品ID</param>
        /// <returns></returns>
        public string GetPlanGoodsInfo(string uniacid, string goodsID)
        {
            try
            {
                var goods = thisData.GetPlanGoodsInfo(uniacid, new ObjectId(goodsID));
                return new BaseResponseModel<GoodsModel>() { StatusCode = Tools.ActionParams.code_ok, JsonData = goods }.ToJson();
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

        /// <summary>
        /// 获取所有套餐类型
        /// </summary>
        /// <returns></returns>
        public string GetAllPlanType(string uniacid)
        {
            try
            {
                List<GoodsType> list = thisData.GetAllPlanType(uniacid);
                return new BaseResponseModel<List<GoodsType>>() { StatusCode = Tools.ActionParams.code_ok, JsonData = list }.ToJson();
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

       
    }
}
