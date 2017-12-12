﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PhotoPrintWXSmall.App_Data;
using PhotoPrintWXSmall.Models;
using System.IO;
using Newtonsoft.Json;
using Tools.Response;
using Tools.ResponseModels;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting.Internal;

namespace PhotoPrintWXSmall.Controllers
{
    public class MerchantController : BaseController<MerchantData, GoodsModel>
    {
        HostingEnvironment hostingEnvironment;
        public MerchantController(HostingEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult GoodsPush()
        {
            return View();
        }
        public IActionResult GoodsManage()
        {
            return View();
        }
        public IActionResult Settings()
        {
            return View();
        }
        /// <summary>
        /// 添加商品类型
        /// </summary>
        /// <returns></returns>
        public string PushGoodsType()
        {
            try
            {
                string json = new StreamReader(Request.Body).ReadToEnd();
                GoodsType goodsType = JsonConvert.DeserializeObject<GoodsType>(json);
                thisData.SaveGoodsType(goodsType);
                return JsonResponseModel.SuccessJson;
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }
        /// <summary>
        /// 添加商品菜单
        /// </summary>
        /// <returns></returns>
        public string PushGoodsMenu()
        {
            try
            {
                string json = new StreamReader(Request.Body).ReadToEnd();
                var goodsMenu = JsonConvert.DeserializeObject<OneGoodsMenu>(json);
                thisData.PushOneGoodsMenu(goodsMenu); return JsonResponseModel.SuccessJson;
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }
        /// <summary>
        /// 获取商品类型列表
        /// </summary>
        /// <param name="typeClass"></param>
        /// <returns></returns>
        public string GetGoodsType(TypeClass typeClass)
        {
            try
            {
                List<GoodsType> typeList = thisData.GetGoodsTypes(typeClass);
                return new BaseResponseModel<List<GoodsType>>() { StatusCode = Tools.ActionParams.code_ok, JsonData = typeList }.ToJson();
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }
        /// <summary>
        /// 是否有该商品
        /// </summary>
        /// <returns></returns>
        public string HasGoods()
        {
            try
            {
                string json = new StreamReader(Request.Body).ReadToEnd();
                var goodsMenu = JsonConvert.DeserializeObject<OneGoodsMenu>(json);
                bool hasGoods = thisData.HasGoods(goodsMenu);
                return new BaseResponseModel<bool>() { StatusCode = Tools.ActionParams.code_ok, JsonData = hasGoods }.ToJson();
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }
        /// <summary>
        /// 添加商品套餐类型
        /// </summary>
        /// <param name="planGoodsType"></param>
        /// <returns></returns>
        public string PushPlanGoodsType(string planGoodsType)
        {
            try
            {
                thisData.PushPlanGoodsType(planGoodsType);
                return JsonResponseModel.SuccessJson;
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

        public string PushPlanGoods()
        {
            try
            {
                string json = new StreamReader(Request.Body).ReadToEnd();
                var goodsModel = JsonConvert.DeserializeObject<GoodsModel>(json);
                thisData.PushPlanGoods(goodsModel); return JsonResponseModel.SuccessJson;
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

        public string SaveGoodsFiles(int goodsType, int picType)
        {
            try
            {
                var files = Request.Form.Files;
                thisData.SaveGoodsFiles(goodsType, picType, files, hostingEnvironment);
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
            return JsonResponseModel.SuccessJson;
        }
    }
}