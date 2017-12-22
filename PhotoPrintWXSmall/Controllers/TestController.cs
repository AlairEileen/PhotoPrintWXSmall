﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tools.ResponseModels;
using We7Tools;
using We7Tools.MysqlTool;

namespace PhotoPrintWXSmall.Controllers
{
    public class TestController : Controller
    {
        public IActionResult GetProcessMiniZip()
        {
            var siteInfo = new SiteInfo()
            {
                acid = "1",
                design_method = "2",
                multiid = "3",
                redirect_module = "4",
                siteroot = "5",
                template = "6",
                title = "7",
                uniacid = "8",
                version = "9"
            };
            string fileUrl;
            ProcessMiniTool.CreateProcessMiniZip(siteInfo, out fileUrl);
            byte[] fileByteArray = System.IO.File.ReadAllBytes(fileUrl);
            var fileName = Path.GetFileName(fileUrl);
            System.IO.File.Delete(fileUrl);
            return File(fileByteArray, "application/vnd.android.package-archive", fileName);
        }

        public string getTestUserList()
        {
            var list = new MysqlDBTool().GetTestUserList();
            return new BaseResponseModel<List<TestUser>>() { JsonData=list }.ToJson();
        }
    }
}
