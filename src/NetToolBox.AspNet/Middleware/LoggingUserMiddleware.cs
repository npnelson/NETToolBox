using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NetToolBox.AspNet.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NetToolBox.AspNet.Middleware
{
    public sealed class LoggingUserMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggerFactory _loggerFactory;
        public LoggingUserMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _loggerFactory = loggerFactory;
        }

        public async Task Invoke(HttpContext context)
        {

            var logger = _loggerFactory.CreateLogger("LoggerEnhancer");
            var loggingScope = new List<KeyValuePair<string, object>>();

            if (context.User != null)
            {
                var userID = context.User?.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
                if (!String.IsNullOrWhiteSpace(userID)) loggingScope.Add(new KeyValuePair<string, object>("UserID", userID));
            }

            using (var scope = logger.BeginScope(loggingScope))
            {
                await _next(context);
            }

        }
    }
}
namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Adds user to log scope
    /// </summary>
    public static class LoggingUserMiddlewareExtensions
    {
        public static IApplicationBuilder UseLoggingUser(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggingUserMiddleware>();
        }
    }
}

