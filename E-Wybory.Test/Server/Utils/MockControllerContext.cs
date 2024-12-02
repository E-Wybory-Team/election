using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Syncfusion.Pdf.Lists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace E_Wybory.Test.Server.Utils
{
    internal static class MockControllerContext
    {
        public static Microsoft.AspNetCore.Mvc.ControllerContext DefaultContext()
        {
            var controllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext();
            controllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            return controllerContext;
        }

        public static Microsoft.AspNetCore.Mvc.ControllerContext WithUser(string username , List<Claim> claims)
        {
            claims.Add(new Claim("name", username));
            var mockIdentity = new ClaimsIdentity(claims, "TestAuthType");
            var mockPrincipal = new ClaimsPrincipal(mockIdentity);

            var mockContext = new Mock<HttpContext>();
            mockContext.Setup(c => c.User).Returns(mockPrincipal);

            var mockControllerContext = new ControllerContext
            {
                HttpContext = mockContext.Object
            };

            return mockControllerContext;
        }
        
    }
}
