using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NetToolBox.AspNet;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Xunit;

namespace NetToolBox.Tests
{
    public class AspNetHelperFunctionTests
    {
        [Fact]
        public void RunningOnLocalHostTest()
        {
            var context = new DefaultHttpContext();
            context.Request.Host = new Microsoft.AspNetCore.Http.HostString("LOCALHOST:5000");

            var result = AspNetHelperFunctions.IsRunningOnLocalHost(context);

            result.Should().BeTrue();

        }

        [Fact]
        public void NotRunningOnLocalHostTest()
        {
            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = IPAddress.Parse("8.8.8.8");
            context.Connection.LocalIpAddress = IPAddress.Parse("127.0.0.1");
            context.Request.Host = new Microsoft.AspNetCore.Http.HostString("localhostother");

            var result = AspNetHelperFunctions.IsRunningOnLocalHost(context);

            result.Should().BeFalse();

        }

        [Fact]
        public void NullRunningOnLocalHostTest()
        {
            var context = new DefaultHttpContext();

            var result = AspNetHelperFunctions.IsRunningOnLocalHost(context);

            result.Should().BeTrue();

        }
    }
}
