using MongoDB.Driver;
using PhotoPrintWXSmall.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoPrintWXSmall.App_Data
{
    public class CompanyData : BaseData<CompanyModel>
    {
        internal decimal GetDefaultCarriage(string uniacid)
        {
            var company = collection.Find(x => x.uniacid.Equals(uniacid)).FirstOrDefault();
            if (company == null || company.OrderProperty == null)
            {
                throw new Exception();
            }
            return company.OrderProperty.DefaultCarriage;
        }

        internal ProcessMiniInfo GetProcessMiniInfo(string uniacid)
        {
            var company = collection.Find(x => x.uniacid.Equals(uniacid)).FirstOrDefault();
            if (company == null || company.ProcessMiniInfo == null)
            {
                return null;
            }
            return company.ProcessMiniInfo;
        }
    }
}
