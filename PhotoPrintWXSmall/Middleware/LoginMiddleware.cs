using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoPrintWXSmall.Middleware
{
    public class LoginMiddleware
    {
        private readonly RequestDelegate _next;

        public LoginMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            byte[] data = context.Session.Get("CompanyUserName");
            if (data == null || string.IsNullOrEmpty(System.Text.Encoding.UTF8.GetString(data)))
            {
                context.Response.Redirect("/");
                return;
            }

            await _next.Invoke(context);
        }

    }
    public static partial class MiddlewareExtensions
    {
        public static IApplicationBuilder UseLoginMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoginMiddleware>();
        }
    }
}
