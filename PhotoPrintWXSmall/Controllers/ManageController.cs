using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PhotoPrintWXSmall.App_Data;
using PhotoPrintWXSmall.Models;
using Tools.DB;
using Tools.Response;
using Tools.ResponseModels;
using We7Tools;
using We7Tools.Extend;
using We7Tools.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PhotoPrintWXSmall.Controllers
{
    public class ManageController : BaseController<ManangeData, ManageModel>
    {
        private IHostingEnvironment hostingEnvironment;
        public ManageController(IHostingEnvironment environment)
            : base(false)
        {
            this.hostingEnvironment = environment;
        }
        // GET: /<controller>/
        public IActionResult Index(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return View();
            }
            ViewData["key"] = key;
            var db = new MongoDBTool().GetMongoCollection<We7Temp>();
            We7Temp data = null;

            if (hostingEnvironment.IsDevelopment())
                data = db.Find(x => x.We7TempID.Equals(new ObjectId(key))).FirstOrDefault();
            else
                data = db.FindOneAndDelete(x => x.We7TempID.Equals(new ObjectId(key)));

            if (data == null)
            {
                return RedirectToAction("Index", "Error");
            }
            ViewData["we7Data"] = data.Data;
            JObject jObject = (JObject)JsonConvert.DeserializeObject(data.Data);
            string uniacid = (string)jObject["uniacid"];
            if (!string.IsNullOrEmpty(uniacid))
            {
                HttpContext.Session.PushWe7Data(data.Data);
            }
            hasIdentity = true;
            return RedirectToAction("Index", "Merchant");
        }

        public string ReceiveWe7Data()
        {
            try
            {
                string json = new StreamReader(Request.Body).ReadToEnd();
                var db = new MongoDBTool().GetMongoCollection<We7Temp>();
                var we7Temp = new We7Temp() { Data = json };
                db.InsertOne(we7Temp);
                return new BaseResponseModel<string>() { StatusCode = Tools.ActionParams.code_ok, JsonData = we7Temp.We7TempID.ToString() }.ToJson();
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
                throw;
            }
        }

        public IActionResult ProcessMiniZipDownload()
        {
            if (!HttpContext.Session.HasWe7Data())
            {
                return RedirectToAction("Index", "Error", new { errorType = ErrorType.ErrorNoUserOrTimeOut });
            }
            try
            {
                string fileUrl;
                ProcessMiniTool.GetProcessMiniZipPath(HttpContext.Session, out fileUrl);
                byte[] fileByteArray = System.IO.File.ReadAllBytes(fileUrl);
                var fileName = Path.GetFileName(fileUrl);
                System.IO.File.Delete(fileUrl);
                return File(fileByteArray, "application/vnd.android.package-archive", fileName);
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Error");
                //throw;
            }
        }
    }
}
