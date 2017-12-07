using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Tools.Json;
using Tools.ResponseModels;

namespace Tools.Response
{
    public class JsonResponseModel
    {
        static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();


        static JsonResponseModel()
        {
            string[] param = new string[] { "StatusCode" };
            jsonSerializerSettings.ContractResolver = new LimitPropsContractResolver(param);
        }

        public static string SuccessJson()
        {
            return JsonConvert.SerializeObject(new BaseResponseModel<string>() { StatusCode = ActionParams.code_ok }, jsonSerializerSettings);
        }
        public static string ErrorJson()
        {
            return JsonConvert.SerializeObject(new BaseResponseModel<string>() { StatusCode = ActionParams.code_error }, jsonSerializerSettings);

        }
    }
}
