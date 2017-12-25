using System;
using System.Collections.Generic;
using System.Text;
using We7Tools.Models;
using WXSmallAppCommon.Models;
using WXSmallAppCommon.WXInteractions;

namespace We7Tools
{
    public class We7Tools
    {
        public static WXAccountInfo GetWeChatUserInfo(string uniacid, string code, string iv, string encryptedData)
        {
            var config = We7ProcessMiniConfig.GetAllConfig(uniacid);
            return WXLoginAction.ProcessRequest(code, iv, encryptedData, config.APPID, config.APPSECRET);
        }

        //public static 
    }
}
