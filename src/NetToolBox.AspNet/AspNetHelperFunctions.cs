using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace NetToolBox.AspNet
{
   public static class AspNetHelperFunctions
    {
        /// <summary>
        /// Returns true if this is running on localhost or hostname is null
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool IsRunningOnLocalHost(HttpContext context)
        {
            //from https://www.strathweb.com/2016/04/request-islocal-in-asp-net-core/
            var connection = context.Connection;
            if (connection.RemoteIpAddress != null)
            {
                if (connection.LocalIpAddress != null)
                {
                    return connection.RemoteIpAddress.Equals(connection.LocalIpAddress);
                }
                else
                {
                    return IPAddress.IsLoopback(connection.RemoteIpAddress);
                }
            }

            // for in memory TestServer or when dealing with default connection info
            if (connection.RemoteIpAddress == null && connection.LocalIpAddress == null)
            {
                return true;
            }

            return false;
        }
    }
}
