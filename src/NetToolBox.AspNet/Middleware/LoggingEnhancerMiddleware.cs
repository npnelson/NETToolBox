using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using NetToolBox.AspNet.Middleware;
using NetToolBox.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetToolBox.AspNet.Middleware
{
  
        public sealed class LoggingEnhancerMiddleware
        {
            private readonly RequestDelegate _next;
            private readonly ILoggerFactory _loggerFactory;
            private readonly string _environmentName;
            public LoggingEnhancerMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment)
            {
                _next = next;
                _loggerFactory = loggerFactory;
                _environmentName = hostingEnvironment.EnvironmentName;
            }

            public async Task Invoke(HttpContext context)
            {
                var version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
                var userAgent = context.Request.Headers.FirstOrDefault(x => x.Key == "User-Agent").Value;
                var logger = _loggerFactory.CreateLogger("LoggerEnhancer");
                var loggingScope = new List<KeyValuePair<string, object>>();
                loggingScope.Add(new KeyValuePair<string, object>("Host", context.Request.Host));
                loggingScope.Add(new KeyValuePair<string, object>("RemoteIP", context.Features.Get<IHttpConnectionFeature>().RemoteIpAddress.ToString()));
                loggingScope.Add(new KeyValuePair<string, object>("UserAgent", userAgent.ToString()));
                loggingScope.Add(new KeyValuePair<string, object>("Method", context.Request.Method));
                loggingScope.Add(new KeyValuePair<string, object>("AppVersion", version));
                loggingScope.Add(new KeyValuePair<string, object>("EnvironmentName", _environmentName));
                var coreClrVersion = CoreClrHelpers.GetCoreClrVersion();
                if (!String.IsNullOrWhiteSpace(coreClrVersion)) loggingScope.Add(new KeyValuePair<string, object>("CoreClrVersion", coreClrVersion));

                if (context.User != null)
                {
                    var userID = context.User.Identity.Name;
                    if (!String.IsNullOrWhiteSpace(userID))
                    {
                        loggingScope.Add(new KeyValuePair<string, object>("UserID", userID));
                    }
                    else
                    {
                        var appID = context.User.FindFirst(x => x.Type == "appid")?.Value;
                        if (!String.IsNullOrWhiteSpace(appID)) loggingScope.Add(new KeyValuePair<string, object>("appid", appID));
                    }
                }
                AddHeaderToLoggingScope(loggingScope, "x-ms-client-request-id", context);
                AddHeaderToLoggingScope(loggingScope, "CorrelationId", context);
                using (var scope = logger.BeginScope(loggingScope))
                {
                    await _next(context);
                }

            }

            private void AddHeaderToLoggingScope(List<KeyValuePair<string, object>> loggingScope, string headerName, HttpContext context)
            {
                var header = context.Request.Headers.FirstOrDefault(x => x.Key.ToUpperInvariant() == headerName.ToUpperInvariant());
                if (!String.IsNullOrWhiteSpace(header.Key))
                {
                    loggingScope.Add(new KeyValuePair<string, object>(headerName, header.Value.ToString()));
                }
            }
        }  
}
namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Adds several desirable properties to MVC logging (host, application version, clientrequestid, correlationid, user agent, userID/Name (if configured), remoteendpoint
    /// </summary>
    public static class LoggingEnhancerMiddlewareExtensions
    {
        public static IApplicationBuilder UseLoggingEnhancer(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggingEnhancerMiddleware>();
        }
    }
}

