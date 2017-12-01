using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NetToolBox.AspNet.Middleware;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NetToolBox.Tests
{
    public class RequireAuthenticationTests
    {
        [Fact]
        public async Task AuthenticatedTest()
        {
            //Arrange
            var context = new DefaultHttpContext();

            context.User = new System.Security.Claims.ClaimsPrincipal();
            context.User.AddIdentity(new System.Security.Claims.ClaimsIdentity("unitTestAuthentication"));
            context.User.AddIdentity(new System.Security.Claims.ClaimsIdentity());


            //just add a delegate so we can test to make sure it got called
            RequestDelegate next = x =>
            {
                x.Response.ContentType = "application/xml";  //just picked an arbitrary non default contenttype
                return Task.FromResult<object>(null);
            };

            var middleware = new RequireAuthenticationMiddleware(next);

            //Act
            await middleware.Invoke(context);

            //Assert

            context.Response.ContentType.Should().Be("application/xml"); //make sure our delegate got called         
        }

        [Fact]

        public async Task NothAuthenticatedShouldReturnUnauthorizedTest()
        {
            //Arrange
            var context = new DefaultHttpContext();

            //just add a delegate 
            RequestDelegate next = x =>
            {
                x.Response.ContentType = "application/xml";  //just picked an arbitrary non default contenttype
                return Task.FromResult<object>(null);
            };

            var middleware = new RequireAuthenticationMiddleware(next);

            //Act
            await middleware.Invoke(context);

            //Assert
            context.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);

        }
    }

}
