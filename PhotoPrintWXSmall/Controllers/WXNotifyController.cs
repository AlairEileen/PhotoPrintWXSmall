using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using PhotoPrintWXSmall.Models;
using Tools.DB;
using WXSmallAppCommon.WXTool;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PhotoPrintWXSmall.Controllers
{
    public class WXNotifyController : Controller
    {

        /// <summary>
        /// 微信支付回掉
        /// </summary>
        /// <returns></returns>
        public string OnWXPayBack()
        {
            var body = Request.Body;

            StringBuilder builder = new StringBuilder();
            using (Stream ins = body)
            {
                int count = 0;
                byte[] buffer = new byte[1024];
                while ((count = body.Read(buffer, 0, 1024)) > 0)
                {
                    builder.Append(Encoding.UTF8.GetString(buffer, 0, count));
                }
            }

            //var bodyString = body.ToString();
            var bodyString = builder.ToString();

            Log.Info(this.GetType().ToString(), "Receive data from WeChat : " + bodyString);
            string ret = "";
            //转换数据格式并验证签名
            WxPayData data = new WxPayData();
            try
            {
                data.FromXml(bodyString);
                OnPaySuccess(data);
            }
            catch (WxPayException ex)
            {
                //若签名错误，则立即返回结果给微信支付后台
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", ex.Message);
                Log.Error(this.GetType().ToString(), "Sign check error : " + res.ToXml());
                ret = res.ToXml();
            }
            Log.Info(this.GetType().ToString(), "Check sign success");
            return ret;
        }

        /// <summary>
        /// 微信支付成功返回数据
        /// </summary>
        /// <param name="data"></param>
        private void OnPaySuccess(WxPayData data)
        {
            try
            {
                var attach = (string)data.GetValue("attach");
                var wxOrderId = (string)data.GetValue("transaction_id");
                if (string.IsNullOrEmpty(attach))
                {
                    return;
                }
                if (string.IsNullOrEmpty(wxOrderId))
                {
                    Console.WriteLine("####微信订单号为空");
                }
                string[] aa = attach.Split(',');
                string accountID = aa[0];
                string orderID = aa[1];
                var filter = Builders<AccountModel>.Filter;
                var filterSum = filter.Eq(x => x.AccountID, new ObjectId(accountID)) & filter.Eq("Orders.OrderID", new ObjectId(orderID));
                var update = Builders<AccountModel>.Update.Set("Orders.$.OrderStatus", OrderStatus.waitingSend).Set("Orders.$.WXOrderId", wxOrderId);
                new MongoDBTool().GetMongoCollection<AccountModel>().UpdateOne(filterSum, update);
            }
            catch (Exception ex)
            {
                Console.WriteLine("腾讯支付成功返回处理出错" + ex.Message);
            }
        }
    }
}
