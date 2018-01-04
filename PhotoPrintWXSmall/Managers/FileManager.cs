using MongoDB.Driver;
using PhotoPrintWXSmall.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tools.DB;

namespace PhotoPrintWXSmall.Managers
{
    public class FileManager
    {
        public string uniacid { get; set; }
        public string filePath { get; set; }
        public string fileName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uniacid">商户识别ID（必须）</param>
        /// <param name="filePath">(上传时)</param>
        /// <param name="fileName">（下载和删除时）</param>
        private FileManager(string uniacid, string filePath, string fileName)
        {
            this.uniacid = uniacid;
            this.filePath = filePath;
            this.fileName = fileName;
        }
        public static Exerciser Exerciser(string uniacid, string filePath, string fileName)
        {
            return new Exerciser(new FileManager(uniacid, filePath, fileName));
        }

    }
    public class Exerciser
    {
        private FileManager fm;
        public Exerciser(FileManager fm) { this.fm = fm; }
        public void SaveFile()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(DoSaveFile));
        }
        public void DelFile()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(DoDelFile));
        }
        public async Task<string> GetFile()
        {
            var company = GetCompany();
            if (company == null || company.QiNiuModel == null)
            {
                return "";
            }
            return await company.QiNiuModel.GetFileUrl(fm.fileName);
        }



        private void DoDelFile(object state)
        {
            var company = GetCompany();
            if (company == null || company.QiNiuModel == null)
            {
                return;
            }
            company.QiNiuModel.DeleteFile(fm.fileName);
        }

        private void DoSaveFile(object state)
        {
            var company = GetCompany();
            if (company == null || company.QiNiuModel == null)
            {
                return;
            }
            company.QiNiuModel.UploadFile(fm.filePath);
        }
        private CompanyModel GetCompany()
        {
            var company = new MongoDBTool().GetMongoCollection<CompanyModel>().Find(x => x.uniacid.Equals(fm.uniacid)).FirstOrDefault();
            return company;
        }
    }
}
