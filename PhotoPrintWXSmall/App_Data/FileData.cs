using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Tools.Models;
using System.Net.Http.Headers;
using Tools;
using System.IO;
using System.Threading;
using MongoDB.Bson;
using PhotoPrintWXSmall.Models;
using MongoDB.Driver;

namespace PhotoPrintWXSmall.App_Data
{
    public class FileData : BaseData<FileModel<string[]>>
    {
        /// <summary>
        /// 用户上传照片
        /// </summary>
        /// <param name="accountID">账户ID</param>
        /// <param name="file"></param>
        /// <returns></returns>
        internal string SaveOneFile(ObjectId accountID, IFormFile file)
        {
            string resultFileId = "";

            long size = 0;

            var filename = ContentDispositionHeaderValue
                                  .Parse(file.ContentDisposition)
                                  .FileName
                                  .Trim('"');
            string saveDir = $@"{ConstantProperty.BaseDir}{ConstantProperty.AlbumDir}";
            string dbSaveDir = $@"{ConstantProperty.AlbumDir}";
            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }
            string exString = filename.Substring(filename.LastIndexOf("."));
            string saveName = Guid.NewGuid().ToString("N");
            filename = $@"{saveDir}{saveName}{exString}";

            size += file.Length;
            FileModel<string[]> fileCard = new FileModel<string[]>();
            using (FileStream fs = System.IO.File.Create(filename))
            {
                file.CopyTo(fs);
                fs.Flush();
                string[] fileUrls = new string[] { $@"{dbSaveDir}{saveName}{exString}" };
            }
            var accountCollection = mongo.GetMongoCollection<AccountModel>();
            var account = accountCollection.Find(x=>x.AccountID.Equals(accountID)).FirstOrDefault();
            if (account.UploadImages==null)
            {
                accountCollection.UpdateOne(x=>x.AccountID.Equals(accountID),Builders<AccountModel>.Update.Set(x=>x.UploadImages,new List<FileModel<string[]>>()));
            }
            ParamsCreate3Img params3Img = new ParamsCreate3Img() { FileName = filename, FileDir = ConstantProperty.AlbumDir };
            params3Img.OnFinish += fileModel =>
            {
                fileModel.FileID = ObjectId.GenerateNewId();
                accountCollection.UpdateOne(x=>x.AccountID.Equals(accountID),
                    Builders<AccountModel>.Update.Push(x=>x.UploadImages,fileModel));
                //mongo.GetMongoCollection<FileModel<string[]>>("FileModel").InsertOne(fileModel);
                resultFileId = fileModel.FileID.ToString();
            };
            //ThreadPool.QueueUserWorkItem(new WaitCallback(ImageTool.Create3Img), params3Img);
            new Thread(new ParameterizedThreadStart(ImageTool.Create3Img)).Start(params3Img);
            return resultFileId;
        }
    }
}
