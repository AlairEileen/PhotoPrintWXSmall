using System;
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
using Microsoft.AspNetCore.Hosting;
using MongoDB.Bson;
using System.IO.Compression;
using We7Tools.Extend;

namespace PhotoPrintWXSmall.Controllers
{
    public class MerchantController : BaseController<MerchantData, GoodsModel>
    {
        IHostingEnvironment hostingEnvironment;
        public MerchantController(IHostingEnvironment hostingEnvironment)
            : base(true)
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
                goodsType.uniacid = HttpContext.Session.GetUniacID();
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
                thisData.PushOneGoodsMenu(HttpContext.Session.GetUniacID(), goodsMenu); return JsonResponseModel.SuccessJson;
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
                List<GoodsType> typeList = thisData.GetGoodsTypes(HttpContext.Session.GetUniacID(), typeClass);
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
                bool hasGoods = thisData.HasGoods(HttpContext.Session.GetUniacID(), goodsMenu);
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
                thisData.PushPlanGoodsType(HttpContext.Session.GetUniacID(), planGoodsType);
                return JsonResponseModel.SuccessJson;
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

        /// <summary>
        /// 添加套餐商品
        /// </summary>
        /// <returns></returns>
        public string PushPlanGoods()
        {
            try
            {
                string json = new StreamReader(Request.Body).ReadToEnd();
                var goodsModel = JsonConvert.DeserializeObject<GoodsModel>(json);
                goodsModel.uniacid = HttpContext.Session.GetUniacID();
                thisData.PushPlanGoods(goodsModel); return JsonResponseModel.SuccessJson;
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

        /// <summary>
        /// 保存商品文件
        /// </summary>
        /// <param name="goodsType"></param>
        /// <param name="picType"></param>
        /// <returns></returns>
        public string SaveGoodsFiles(GoodsClass goodsType, int picType)
        {
            try
            {
                var files = Request.Form.Files;
               thisData.SaveGoodsFiles(HttpContext.Session.GetUniacID(), goodsType, picType, files, hostingEnvironment);
                //thisData.ResetGoodsPics(HttpContext.Session.GetUniacID(), goodsType);
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
            return JsonResponseModel.SuccessJson;
        }

        /// <summary>
        /// 删除商品图片
        /// </summary>
        /// <param name="goodsType">商品类型</param>
        /// <param name="picType">图片类型（0：轮播图，1：详情图）</param>
        /// <returns></returns>
        public string DelGoodsFiles(GoodsClass goodsType, int picType)
        {
            try
            {
                thisData.DelGoodsFiles(HttpContext.Session.GetUniacID(), goodsType, picType);
                return JsonResponseModel.SuccessJson;
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

        /// <summary>
        /// 获取所有订单
        /// </summary>
        /// <returns></returns>
        public string GetAllOrders(OrderStatus orderStatus, int downloaded = -1)
        {
            try
            {
                List<Order> orders = thisData.GetAllOrders(HttpContext.Session.GetUniacID(), orderStatus, downloaded);
                return new BaseResponseModel<List<Order>>() { StatusCode = Tools.ActionParams.code_ok, JsonData = orders }.ToJson();
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

        /// <summary>
        /// 获取订单相关文件
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        public IActionResult GetOrderFile(string orderID)
        {
            var zipFile = thisData.GetOrderFile(new ObjectId(orderID));
            var stream = System.IO.File.OpenRead(zipFile);
            return File(stream, "application/vnd.android.package-archive", Path.GetFileName(zipFile));
        }

        [HttpPatch]
        public string PatchCompanyUser()
        {
            try
            {
                string json = new StreamReader(Request.Body).ReadToEnd();
                var model = JsonConvert.DeserializeObject<LoginViewModel>(json);
                if (!model.VerifyPassword.Equals(model.CompanyUser.CompanyUserPassword))
                {
                    return JsonResponseModel.NullJson;
                }
                thisData.PatchCompanyUser(model.CompanyUser, model.OldPassword);
                return JsonResponseModel.SuccessJson;
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

        public string GetAllGoods(GoodsClass goodsClass)
        {
            try
            {
                List<GoodsModel> list = thisData.GetAllGoods(HttpContext.Session.GetUniacID(), goodsClass);
                return new BaseResponseModel<List<GoodsModel>>() { StatusCode = Tools.ActionParams.code_ok, JsonData = list }.ToJson();
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

        public async Task<string> SavePlanGoodsListPic()
        {
            try
            {
                var files = Request.Form.Files;
                return await thisData.SavePlanGoodsListPic(HttpContext.Session.GetUniacID(), files[0]);
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

        public string SendOrder(string orderID, string company, string number)
        {
            try
            {
                if (string.IsNullOrEmpty(company) || string.IsNullOrEmpty(number))
                {
                    return JsonResponseModel.ErrorNullJson;
                }
                thisData.SendOrder(new ObjectId(orderID), company, number);
                return JsonResponseModel.SuccessJson;
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

        public string DelGoods(string goodsID)
        {
            try
            {
                thisData.DelGoods(new ObjectId(goodsID));
                return JsonResponseModel.SuccessJson;
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

    }
}