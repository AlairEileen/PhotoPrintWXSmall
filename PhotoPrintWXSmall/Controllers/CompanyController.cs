using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PhotoPrintWXSmall.App_Data;
using PhotoPrintWXSmall.Models;
using Tools.Response;
using Tools.ResponseModels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PhotoPrintWXSmall.Controllers
{
    public class CompanyController : BaseController<CompanyData, CompanyModel>
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 获取运费
        /// </summary>
        /// <param name="uniacid">公司识别ID</param>
        /// <returns></returns>
        public string GetDefaultCarriage(string uniacid)
        {
            try
            {
                return new BaseResponseModel<decimal>() { StatusCode = Tools.ActionParams.code_ok, JsonData = thisData.GetDefaultCarriage(uniacid) }.ToJson();
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }
     
        /// <summary>
        /// 获取小程序信息
        /// </summary>
        /// <param name="uniacid"></param>
        /// <returns></returns>
        public string GetProcessMiniInfo(string uniacid)
        {
            try
            {
                var info = thisData.GetProcessMiniInfo(uniacid);
                return new BaseResponseModel<ProcessMiniInfo>() { StatusCode = info == null ? Tools.ActionParams.code_null : Tools.ActionParams.code_ok, JsonData = info }.ToJson();
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

    }
}
