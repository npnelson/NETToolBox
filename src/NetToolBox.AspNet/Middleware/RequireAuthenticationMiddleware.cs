﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetToolBox.AspNet.Middleware
{
    public sealed class RequireAuthenticationMiddleware
    {

        private readonly RequestDelegate _next;
        public RequireAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {           
            var anyAuthenticated = false;
            var identities = context?.User?.Identities;
            if (identities != null) anyAuthenticated = identities.Any(x => x.IsAuthenticated);

            if ( !anyAuthenticated)
            {               
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            else
            {
                await _next(context);
            }

        }
    }


    public static class RequireAuthenticationExceptForLocalHostMiddlewareExtensions
    {
        /// <summary>
        /// Requires Authentication for anything after the call in the pipeline
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseRequireAuthenticationExceptForLocalHostMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>().UseMiddleware<RequireAuthenticationMiddleware>();
        }
    }

}