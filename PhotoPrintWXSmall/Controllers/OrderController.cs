using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;
using PhotoPrintWXSmall.App_Data;
using PhotoPrintWXSmall.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tools.Response;

namespace PhotoPrintWXSmall.Controllers
{
    public class OrderController:BaseController<OrderData,AccountModel>
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
                thisData.PushShoppingCart(new ObjectId(accountID),shop);
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
