using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PhotoPrintWXSmall.Controllers
{
    public class MerchantController : Controller
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