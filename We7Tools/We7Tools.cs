﻿using System;
using System.Collections.Generic;
using System.Text;
using We7Tools.Models;
using WXSmallAppCommon.Models;
using WXSmallAppCommon.WXInteractions;
using WXSmallAppCommon.WXTool;

namespace We7Tools
{
    public static class We7Tools
    {
        public static WXAccountInfo GetWeChatUserInfo(string uniacid, string code, string iv, string encryptedData)
        {
            var config = We7ProcessMiniConfig.GetAllConfig(uniacid);
            return WXLoginAction.ProcessRequest(code, iv, encryptedData, config.APPID, config.APPSECRET);
        }

        /// <summary>
        /// 微擎统一下单
        /// </summary>
        /// <param name="jsApiPay"></param>
        /// <param name="uniacid">商户区别ID</param>
        /// <param name="body"></param>
        /// <param name="attach"></param>
        /// <param name="goods_tag"></param>
        /// <returns></returns>
        public static WxPayData CreateWeChatOrder(this JsApiPay jsApiPay, string uniacid, string body, string attach, string goods_tag)
        {
            ProcessMiniConfig pmc = We7ProcessMiniConfig.GetAllConfig(uniacid);
            return jsApiPay.GetUnifiedOrderResult(body, attach, goods_tag, pmc.APPID, pmc.MCHID);
        }
      
        /// <summary>
        /// 微擎退款
        /// </summary>
        /// <param name="uniacid">商户区别ID</param>
        /// <param name="transaction_id">微信订单号</param>
        /// <param name="out_trade_no">商户订单号</param>
        /// <param name="total_fee">订单总金额</param>
        /// <param name="refund_fee">退款金额</param>
        /// <returns></returns>
        public static string RunRefund(string uniacid,string transaction_id,string out_trade_no,int total_fee,int refund_fee)
        {
            ProcessMiniConfig pmc = We7ProcessMiniConfig.GetAllConfig(uniacid);
            return Refund.Run(transaction_id, out_trade_no, total_fee, refund_fee,pmc. APPID,pmc. MCHID,pmc.KEY, pmc.SSLCERT_PASSWORD, pmc.SSLCERT_PATH);
        }
    }
}
