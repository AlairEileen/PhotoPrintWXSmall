using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tools.Models;

namespace PhotoPrintWXSmall.Models
{
    public class AccountModel : BaseAccount
    {
        public string OpenID { get; set; }
        public List<OrderLocation> OrderLocations { get; set; }
    }

    public class OrderLocation
    {
    }
}
