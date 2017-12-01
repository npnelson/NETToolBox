using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NetToolBox.AspNet.Middleware;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NetToolBox.AspNet.Middleware
{
    public sealed class RequireHttpsExceptForLocalHostMiddleware
    {

        private readonly RequestDelegate _next;
        public RequireHttpsExceptForLocalHostMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.IsHttps || AspNetHelperFunctions.IsRunningOnLocalHost(context))
            {
                await _next(context);
            }
            else
            {
                var request = context.Request;
                var newUrl = string.Concat(
                   "https://",
                   request.Host.ToUriComponent(),
                   request.PathBase.ToUriComponent(),
                   request.Path.ToUriComponent(),
                   request.QueryString.ToUriComponent());

                context.Response.Redirect(newUrl);
            }          
        }
    }   
}

namespace Microsoft.AspNetCore.Builder
{
    public static class RequireHttpsExceptForLocalHostMiddlewareExtensions
    {
        /// <summary>
        /// This is useful in webapi scenarioes where you want to require authentication except for when testing on localhost
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseRequireHttpsExceptForLocalHost(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequireHttpsExceptForLocalHostMiddleware>();
        }
    }
}