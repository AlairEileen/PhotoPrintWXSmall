using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PhotoPrintWXSmall.Models;
using PhotoPrintWXSmall.App_Data;
using System.Text;

namespace PhotoPrintWXSmall.Controllers
{
    public class HomeController : BaseController<HomeData, CompanyModel>
    {


        public IActionResult Index()
        {
            LoginViewModel loginViewModel = new LoginViewModel()
            {
                HasCompanyUser = thisData.HasCompanyUser()
            };
            return View(loginViewModel);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult DoLogin(LoginViewModel loginViewModel)
        {
            if (!loginViewModel.HasCompanyUser && !loginViewModel.VerifyPassword.Equals(loginViewModel.CompanyUser.CompanyUserPassword))
            {
                loginViewModel.ErrorVerify = true;
                return View("Index", loginViewModel);
            }
            if (!loginViewModel.HasCompanyUser)
            {
                thisData.PushCompanyUser(loginViewModel.CompanyUser);
            }
            else
            {
                var status = thisData.HasCompanyUser(loginViewModel.CompanyUser);
                if (!status)
                {
                    loginViewModel.ErrorAccount = true;
                    return View("Index", loginViewModel);
                }
                HttpContext.Session.Set("CompanyUserName", Encoding.UTF8.GetBytes(loginViewModel.CompanyUser.CompanyUserName));

            }

            return Redirect("/Merchant");
        }
    }
}
