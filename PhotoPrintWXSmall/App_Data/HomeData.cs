using MongoDB.Bson;
using MongoDB.Driver;
using PhotoPrintWXSmall.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoPrintWXSmall.App_Data
{
    public class HomeData : BaseData<CompanyModel>
    {
        internal bool HasCompanyUser(CompanyUser companyUser=null)
        {
            var filter = Builders<CompanyModel>.Filter;
            var filterSum = filter.Empty;
            if (companyUser!=null)
            {
                filterSum = filter.Eq("CompanyUsers.CompanyUserName", companyUser.CompanyUserName) & filter.Eq("CompanyUsers.CompanyUserPassword",companyUser.CompanyUserPassword);
            }
            var company = collection.Find(Builders<CompanyModel>.Filter.Empty).FirstOrDefault();
            if (company != null && company.CompanyUsers != null && company.CompanyUsers.Count > 0)
            {
                return true;
            }
            return false;
        }

        internal void PushCompanyUser(CompanyUser companyUser)
        {
            var filter = Builders<CompanyModel>.Filter;
            var company = collection.Find(filter.Empty).FirstOrDefault();
            if (company==null)
            {
                company = new CompanyModel() { };
                collection.InsertOne(company);
            }
            if (company.CompanyUsers == null)
            {
                collection.UpdateOne(x => x.CompanyID.Equals(company.CompanyID),
                    Builders<CompanyModel>.Update.Set(x => x.CompanyUsers, new List<CompanyUser>()));
            }
            companyUser.CompanyUserID = ObjectId.GenerateNewId();
            collection.UpdateOne(x => x.CompanyID.Equals(company.CompanyID),
                Builders<CompanyModel>.Update.Push(x => x.CompanyUsers, companyUser));
        }

        
    }
}
