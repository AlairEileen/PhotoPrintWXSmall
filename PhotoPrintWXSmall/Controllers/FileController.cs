using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using PhotoPrintWXSmall.App_Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Tools;
using Tools.Models;
using Tools.Response;
using Tools.ResponseModels;

namespace PhotoPrintWXSmall.Controllers
{
    public class FileController : BaseController<FileData, FileModel<string[]>>
    {
        private IHostingEnvironment hostingEnvironment;
        public FileController(IHostingEnvironment environment)
        {
            this.hostingEnvironment = environment;
        }

        /// <summary>
        /// 文件下载
        /// </summary>
        /// <param name="fileId">文件id</param>
        [HttpGet]
        public IActionResult FileDownload(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
            {
                return null;
            }
            fileUrl = ConstantProperty.BaseDir + fileUrl;
            var stream = System.IO.File.OpenRead(fileUrl);
            return File(stream, "application/vnd.android.package-archive", Path.GetFileName(fileUrl));
        }


        /// <summary>
        /// 上传单张图片
        /// </summary>
        /// <param name="accountID">账户ID</param>
        /// <returns></returns>
        public async Task<string> UploadImage(string accountID)
        {
            var files = Request.Form.Files;
            string resultFileId = null;
            BaseResponseModel<string> responseModel = new BaseResponseModel<string>();
            try 
            {
                resultFileId = await thisData.SaveOneFile(new ObjectId(accountID),files[0]);
                if (string.IsNullOrEmpty(resultFileId))
                {
                    return JsonResponseModel.ErrorJson;
                }
                responseModel.JsonData = resultFileId;
                responseModel.StatusCode = ActionParams.code_ok;
            }
            catch (Exception)
            {
                return JsonResponseModel.ErrorJson;
            }
            return resultFileId;
        }

    }
}
