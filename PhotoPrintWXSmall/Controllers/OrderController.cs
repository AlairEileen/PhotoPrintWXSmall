using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;
using PhotoPrintWXSmall.App_Data;
using PhotoPrintWXSmall.Models;
using System;
using System.Collections.Generic;
using System.IO;
using Tools;
using Tools.Json;
using Tools.Response;
using Tools.ResponseModels;
using We7Tools;
using WXSmallAppCommon.Models;
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
        public string PushShoppingCart(string uniacid, string accountID)
        {
            try
            {
                string json = new StreamReader(Request.Body).ReadToEnd();
                Shop shop = JsonConvert.DeserializeObject<Shop>(json);
                thisData.PushShoppingCart(uniacid, new ObjectId(accountID), shop);
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
        public string ChangeShoppingCartGoodsNum(string uniacid, string accountID, string shopID, int num)
        {
            try
            {
                thisData.ChangeShoppingCartGoodsNum(uniacid, new ObjectId(accountID), new ObjectId(shopID), num);
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
        public string DelShopInCart(string uniacid, string accountID, string shopID)
        {
            try
            {
                thisData.DelShopInCart(uniacid, new ObjectId(accountID), new ObjectId(shopID));
                return JsonResponseModel.SuccessJson;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

        /// <summary>
        /// 获取购物车
        /// </summary>
        /// <param name="accountID"></param>
        /// <returns></returns>
        public string GetShoppingCart(string uniacid, string accountID)
        {
            try
            {
                List<Shop> shoppingCart = thisData.GetShoppingCart(uniacid, new ObjectId(accountID));
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
        public string PushOrder(string uniacid, string accountID, string orderLocationID)
        {
            try
            {
                string json = new StreamReader(Request.Body).ReadToEnd();
                List<Shop> shopList = JsonConvert.DeserializeObject<List<Shop>>(json);
                if (shopList==null||shopList.Count==0)
                {
                    return JsonResponseModel.ErrorNullJson;
                }
                AccountModel account;
                var order = thisData.PushOrder(uniacid, new ObjectId(accountID), new ObjectId(orderLocationID), shopList, out account);
                var wxpm = GetPayParam(uniacid, account, order);
                return new BaseResponseModel<WXPayModel>() { StatusCode = ActionParams.code_ok, JsonData = wxpm }.ToJson();
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

        private WXPayModel GetPayParam(string uniacid, AccountModel account, Order order)
        {
            ///微擎相关
            JsApiPay jsApiPay = new JsApiPay();
            jsApiPay.openid = account.OpenID;
            jsApiPay.total_fee = order.OrderPrice.ConvertToMoneyCent();
            var body = "test";
            var attach =account.AccountID+","+ order.OrderID.ToString();
            var goods_tag = order.ShopList[0].ShopID.ToString();
            jsApiPay.CreateWeChatOrder(uniacid, body, attach, goods_tag);
            var param = jsApiPay.GetJsApiParameters();
            var wxpm = JsonConvert.DeserializeObject<WXPayModel>(param);
            return wxpm;
        }

        /// <summary>
        /// 已生成订单再次支付
        /// </summary>
        /// <param name="uniacid">商家区别ID</param>
        /// <param name="accountID">账户ID</param>
        /// <param name="orderID">订单ID</param>
        /// <returns></returns>
        public string StartPay(string uniacid, string accountID, string orderID)
        {
            try
            {
                AccountModel account;
                Order order;
                thisData.GetOrderAndAccount(uniacid,new ObjectId(accountID),new ObjectId(orderID), out account, out order);
                var wxpm = GetPayParam(uniacid, account, order);
                return new BaseResponseModel<WXPayModel>() { StatusCode = ActionParams.code_ok, JsonData = wxpm }.ToJson();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
        public string GetOrderList(string uniacid, string accountID, int orderStatus = -2)
        {
            try
            {
                List<Order> list = thisData.GetOrderList(uniacid, new ObjectId(accountID), orderStatus);
                JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
                string[] param = new string[] {
                    "StatusCode",
                    "JsonData",
                    "OrderID",
                    "OrderNumber",
                    "wxOrderId",
                    "ShopList",
                    "CreateTime",
                    "OrderPrice",
                    "OrderStatus",
                    "Carriage",
                    "Goods",
                    "ShopImages",
                    "HeaderPics",
                    "FileUrlData",
                    "Title",
                    "GoodsCount",
                    "GoodsPrice"
                };
                jsonSerializerSettings.ContractResolver = new LimitPropsContractResolver(param);
                string aa = JsonConvert.SerializeObject(new BaseResponseModel<List<Order>>() { StatusCode = Tools.ActionParams.code_ok, JsonData = list }, jsonSerializerSettings);
                return aa;
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
        public string ChangeOrderStatus(string uniacid, string orderID, OrderStatus orderStatus)
        {
            try
            {
                thisData.ChangeOrderStatus(uniacid, new ObjectId(orderID), orderStatus);
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
        public string GetShoppingCartList(string uniacid, string accountID)
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
                var list = thisData.GetShoppingCartList(uniacid, new ObjectId(accountID), idList);
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
