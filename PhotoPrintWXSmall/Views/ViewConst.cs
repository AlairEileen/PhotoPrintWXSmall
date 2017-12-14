using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoPrintWXSmall.Views
{
    public class ViewConst
    {
        public static bool CheckRout(ViewDataDictionary<dynamic> viewData, RoutType routType)
        {
            return (RoutType)viewData["routType"] == routType;
        }
        public static void SetRoutType<T>(ViewDataDictionary<T> viewData, RoutType routType)
        {
            viewData["routType"] = routType;
            viewData["Title"] = routType.ToString();
        }

    }

    public enum RoutType
    {
        登录 = -1, 订单 = 0, 商品添加 = 1, 设置 = 2, 商品管理 = 3
    }
}
