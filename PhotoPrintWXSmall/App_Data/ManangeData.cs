using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using PhotoPrintWXSmall.Managers;
using PhotoPrintWXSmall.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Tools;
using Tools.Models;

namespace PhotoPrintWXSmall.App_Data
{
    public class ManangeData : BaseData<ManageModel>
    {
        internal void SetDefaultCarriage(string uniacid, decimal carriage)
        {
            var companyCollection = mongo.GetMongoCollection<CompanyModel>();
            var companyFilter = Builders<CompanyModel>.Filter.Eq(x => x.uniacid, uniacid);
            var company = companyCollection.Find(companyFilter).FirstOrDefault();
            if (company == null)
            {
                companyCollection.InsertOne(new CompanyModel() { uniacid = uniacid });
            }
            var OrderPropertyUpdate = Builders<CompanyModel>.Update.Set(x => x.OrderProperty.DefaultCarriage, carriage);
            if (company.OrderProperty == null)
            {
                OrderPropertyUpdate = Builders<CompanyModel>.Update.Set(x => x.OrderProperty, new OrderProperty() { DefaultCarriage = carriage });
            }
            companyCollection.UpdateOne(companyFilter, OrderPropertyUpdate);
        }

        internal void SetProcessMiniInfo(string uniacid, ProcessMiniInfo processMiniInfo)
        {
            var fileModel = mongo.GetMongoCollection<FileModel<string[]>>("FileModel").Find(x => x.FileID.Equals(processMiniInfo.Logo.FileID)).FirstOrDefault();
            processMiniInfo.Logo = fileModel ?? throw new Exception();
            var companyCollection = mongo.GetMongoCollection<CompanyModel>();
            var companyFilter = Builders<CompanyModel>.Filter.Eq(x => x.uniacid, uniacid);
            var company = companyCollection.Find(companyFilter).FirstOrDefault();
            if (company == null)
            {
                companyCollection.InsertOne(new CompanyModel() { uniacid = uniacid });
            }
            companyCollection.UpdateOne(companyFilter, Builders<CompanyModel>.Update.Set(x => x.ProcessMiniInfo, processMiniInfo));
        }

        internal async Task<string> SaveProcessMiniLogo(string uniacid, IFormFile file)
        {
            string resultFileId = "";

            long size = 0;

            var filename = ContentDispositionHeaderValue
                                  .Parse(file.ContentDisposition)
                                  .FileName
                                  .Trim('"');
            string dbSaveDir = $@"{ConstantProperty.LogoImagesDir}{uniacid}/";
            string saveDir = $@"{ConstantProperty.BaseDir}{dbSaveDir}/";
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
            return await Task.Run(() =>
            {
                ParamsCreate3Img params3Img = new ParamsCreate3Img() { FileName = filename, FileDir = dbSaveDir };
                params3Img.OnFinish += fileModel =>
                {

                    FileManager.Exerciser(uniacid, null, null).SaveFileModel(fileModel);

                    mongo.GetMongoCollection<FileModel<string[]>>("FileModel").InsertOne(fileModel);
                    resultFileId = fileModel.FileID.ToString();

                };
                ImageTool.Create3Img(params3Img);
                return resultFileId;
            });
        }

        internal CompanyModel GetCompanyModel(string uniacid)
        {
            var companyModel = mongo.GetMongoCollection<CompanyModel>().Find(x => x.uniacid.Equals(uniacid)).FirstOrDefault();
            return companyModel;
        }

      

        internal void SetQiNiu(string uniacid, QiNiuModel qiNiuModel)
        {
            var companyCollection = mongo.GetMongoCollection<CompanyModel>();
            companyCollection.UpdateOne(x=>x.uniacid.Equals(uniacid),Builders<CompanyModel>.Update.Set(x=>x.QiNiuModel,qiNiuModel));
        }
    }
}
