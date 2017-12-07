using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PhotoPrintWXSmall.App_Data;
using PhotoPrintWXSmall.Models;

namespace PhotoPrintWXSmall.Controllers
{
    public class MerchantController : BaseController<MerchantData,GoodsModel>
    {
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
    }
}