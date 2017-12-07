using Microsoft.AspNetCore.Mvc;
using PhotoPrintWXSmall.App_Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoPrintWXSmall.Controllers
{
    public class BaseController<T,P> : Controller where T : BaseData<P>
    {
        protected T thisData;
        protected BaseController()
        {
            thisData = System.Activator.CreateInstance<T>();
        }
    }
}
