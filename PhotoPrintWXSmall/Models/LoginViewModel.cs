using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoPrintWXSmall.Models
{
    public class LoginViewModel
    {
        public CompanyUser CompanyUser { get; set; }
        public bool HasCompanyUser { get; set; }
        public string VerifyPassword { get; set; }

        public bool ErrorAccount { get; set; }
        public bool ErrorVerify { get; set; }
    }
}
