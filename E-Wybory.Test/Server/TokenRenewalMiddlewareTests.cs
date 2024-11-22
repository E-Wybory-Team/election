using E_Wybory.Middleware;
using E_Wybory.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace E_Wybory.Tests
{
    public class TokenRenewalMiddlewareTests
    {
        [Fact]
        public async Task InvokeAsync_ShouldRenewToken_WhenTokenIsValid()
        {
            // Arrange
            var mockTokenService = new Mock<IJWTService>();
            mockTokenService.Setup(service => service.RenewToken(It.IsAny<JsonWebToken>()))
                .ReturnsAsync("newToken");

            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = "Bearer oldToken";

            var middleware = new TokenRenewalMiddleware(next: (innerHttpContext) => Task.CompletedTask, tokenService: mockTokenService.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal("Bearer newToken", context.Response.Headers["Authorization"]);
        }
    }
}
