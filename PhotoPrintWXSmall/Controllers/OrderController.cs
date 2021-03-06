﻿using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;
using PhotoPrintWXSmall.App_Data;
using PhotoPrintWXSmall.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tools;
using Tools.Response;
using Tools.ResponseModels;
using We7Tools;
using WXSmallAppCommon.WXInteractions;

namespace PhotoPrintWXSmall.Controllers
{
    public class OrderController : BaseController<OrderData, AccountModel>
    {

        /// <summary>
        /// 加入购物车接口
        /// </summary>
        /// <param name="accountID">账户ID</param>
        /// <returns></returns>
        public string PushShoppingCart(string accountID)
        {
            try
            {
                string json = new StreamReader(Request.Body).ReadToEnd();
                Shop shop = JsonConvert.DeserializeObject<Shop>(json);
                thisData.PushShoppingCart(new ObjectId(accountID), shop);
                return JsonResponseModel.SuccessJson;
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

        /// <summary>
        /// 修改购物车商品数量
        /// </summary>
        /// <param name="accountID">账户ID</param>
        /// <param name="shopID">购物车商品ID</param>
        /// <param name="num">商品数量</param>
        /// <returns></returns>
        public string ChangeShoppingCartGoodsNum(string accountID, string shopID, int num)
        {
            try
            {
                thisData.ChangeShoppingCartGoodsNum(new ObjectId(accountID), new ObjectId(shopID), num);
                return JsonResponseModel.SuccessJson;
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

        /// <summary>
        /// 删除购物车商品
        /// </summary>
        /// <param name="accountID">账户ID</param>
        /// <param name="shopID">购物车商品ID</param>
        /// <returns></returns>
        public string DelShopInCart(string accountID, string shopID)
        {
            try
            {
                thisData.DelShopInCart(new ObjectId(accountID), new ObjectId(shopID));
                return JsonResponseModel.SuccessJson;
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

        /// <summary>
        /// 获取购物车
        /// </summary>
        /// <param name="accountID"></param>
        /// <returns></returns>
        public string GetShoppingCart(string accountID)
        {
            try
            {
                List<Shop> shoppingCart = thisData.GetShoppingCart(new ObjectId(accountID));
                return new BaseResponseModel<List<Shop>>() { StatusCode = Tools.ActionParams.code_ok, JsonData = shoppingCart }.ToJson();
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

        /// <summary>
        /// 创建订单
        /// </summary>
        /// <param name="accountID">账户ID</param>
        /// <param name="orderLocationID">订单地址ID</param>
        /// <returns></returns>
        public string PushOrder(string accountID, string orderLocationID)
        {

            try
            {

                string json = new StreamReader(Request.Body).ReadToEnd();
                List<Shop> shopList = JsonConvert.DeserializeObject<List<Shop>>(json);
                AccountModel account;
                var order = thisData.PushOrder(new ObjectId(accountID), new ObjectId(orderLocationID), shopList, out account);

                //JsApiPay jsApiPay = new JsApiPay();
                //jsApiPay.openid = account.OpenID;
                //jsApiPay.total_fee = order.OrderPrice.ConvertToMoneyCent();
                //var body = "test";
                //var attach = order.OrderID.ToString();
                //var goods_tag = order.ShopList[0].ShopID.ToString();
                //jsApiPay.CreateWeChatOrder(body, attach, goods_tag);
                //var param = jsApiPay.GetJsApiParameters();

                return JsonResponseModel.SuccessJson;
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

        /// <summary>
        /// 获取订单列表
        /// </summary>
        /// <param name="accountID">账户ID</param>
        /// <param name="orderStatus">（-2：全部，-1：失效，0：待付款，1：待发货，2：待收货，3：待评价，4：完成）</param>
        /// <returns></returns>
        public string GetOrderList(string accountID, int orderStatus = -2)
        {
            try
            {
                List<Order> list = thisData.GetOrderList(new ObjectId(accountID), orderStatus);
                return new BaseResponseModel<List<Order>>() { StatusCode = Tools.ActionParams.code_ok, JsonData = list }.ToJson();

            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

        /// <summary>
        /// 修改订单状态
        /// </summary>
        /// <param name="orderID">订单ID</param>
        /// <param name="orderStatus">订单状态：（0：待付款，2：待收货，4：完成）</param>
        /// <returns></returns>
        public string ChangeOrderStatus(string orderID, OrderStatus orderStatus)
        {
            try
            {
                thisData.ChangeOrderStatus(new ObjectId(orderID), orderStatus);
                return JsonResponseModel.SuccessJson;
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

        /// <summary>
        /// 获取下单列表（根据shopID集合获取）
        /// </summary>
        /// <param name="accountID">账户ID</param>
        /// <returns></returns>
        public string GetShoppingCartList(string accountID)
        {
            try
            {
                string json = new StreamReader(Request.Body).ReadToEnd();
                List<string> shopIDList = JsonConvert.DeserializeObject<List<string>>(json);
                List<ObjectId> idList = new List<ObjectId>();
                foreach (var item in shopIDList)
                {
                    idList.Add(new ObjectId(item));
                }
                var list = thisData.GetShoppingCartList(new ObjectId(accountID), idList);
                return new BaseResponseModel<List<Shop>>() { StatusCode = Tools.ActionParams.code_ok, JsonData = list }.ToJson();
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }
    }
}
