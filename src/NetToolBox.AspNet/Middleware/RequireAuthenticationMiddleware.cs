using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NetToolBox.AspNet.Middleware;
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
        private readonly bool _bypassForLocalHost;

        public RequireAuthenticationMiddleware(RequestDelegate next,bool bypassForLocalHost)
        {
            _next = next;
            _bypassForLocalHost = bypassForLocalHost;
        }

        public async Task Invoke(HttpContext context)
        {           
            var anyAuthenticated = false;
            var identities = context?.User?.Identities;
            if (identities != null) anyAuthenticated = identities.Any(x => x.IsAuthenticated);

            if ( !anyAuthenticated & !(_bypassForLocalHost && AspNetHelperFunctions.IsRunningOnLocalHost(context)))
            {               
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            else
            {
                await _next(context);
            }

        }
    }
}
namespace Microsoft.AspNetCore.Builder
{
    public static class RequireAuthenticationMiddlewareExtensions
    {
        /// <summary>
        /// Requires Authentication for anything after the call in the pipeline
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseRequireAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>().UseMiddleware<RequireAuthenticationMiddleware>(false);
        }
        /// <summary>
        /// Requires Authentication unless browser is connecting on localhost
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>

        public static IApplicationBuilder UseRequireAuthenticationExceptForRunningOnLocalHost(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>().UseMiddleware<RequireAuthenticationMiddleware>(true);
        }
    }
}

