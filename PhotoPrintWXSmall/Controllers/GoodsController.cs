﻿using MongoDB.Bson;
using PhotoPrintWXSmall.App_Data;
using PhotoPrintWXSmall.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tools.Response;
using Tools.ResponseModels;

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
        public string GetGoodsOne(string selectedPrintTypeID, string selectedPaperTypeID, string selectedSizeTypeID)
        {
            try
            {
                OneGoodsMenu goodsMenu = thisData.GetGoodsMenu(
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
    }
}
