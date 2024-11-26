using E_Wybory.Middleware;
using E_Wybory.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;
using NuGet.Common;
using System;
using System.Threading.Tasks;
using Xunit;

namespace E_Wybory.Test.Server.Middleware
{
    public class TokenRenewalMiddlewareTests
    {
        [Fact]
        public async Task InvokeAsync_ShouldRenewToken_WhenTokenIsValidAndNearExpiration()
        {
            // Arrange
            var mockTokenService = new Mock<IJWTService>();
            mockTokenService.Setup(service => service.RenewToken(It.IsAny<JsonWebToken>()))
                .ReturnsAsync("newToken");

            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = "Bearer validToken";

            var middleware = new TokenRenewalMiddleware(next: (innerHttpContext) => Task.CompletedTask, tokenService: mockTokenService.Object);

            // Mock the token handler and token
            var mockTokenHandler = new Mock<JsonWebTokenHandler>();
            mockTokenHandler.Setup(handler => handler.CanReadToken("validToken")).Returns(true);
            mockTokenHandler.Setup(handler => handler.ReadJsonWebToken("validToken"))
                .Returns(new JsonWebToken("validToken"));


            var token = mockTokenHandler.Object.ReadJsonWebToken("validToken");
            token.GetType().GetProperty("ValidFrom").SetValue(token, DateTime.UtcNow.AddMinutes(-10));
            token.GetType().GetProperty("ValidTo").SetValue(token, DateTime.UtcNow.AddMinutes(50));

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal("Bearer newToken", context.Response.Headers["Authorization"]);
        }

        [Fact]
        public async Task InvokeAsync_ShouldNotRenewToken_WhenTokenIsNotNearExpiration()
        {
            // Arrange
            var mockTokenService = new Mock<IJWTService>();

            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = "Bearer validToken";

            var middleware = new TokenRenewalMiddleware(next: (innerHttpContext) => Task.CompletedTask, tokenService: mockTokenService.Object);

            // Mock the token handler and token
            var mockTokenHandler = new Mock<JsonWebTokenHandler>();
            mockTokenHandler.Setup(handler => handler.CanReadToken("validToken")).Returns(true);
            mockTokenHandler.Setup(handler => handler.ReadJsonWebToken("validToken"))
                .Returns(new JsonWebToken("validToken"));

            var token = mockTokenHandler.Object.ReadJsonWebToken("validToken");
            token.GetType().GetProperty("ValidFrom").SetValue(token, DateTime.UtcNow.AddMinutes(-10));
            token.GetType().GetProperty("ValidTo").SetValue(token, DateTime.UtcNow.AddMinutes(50));

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.False(context.Response.Headers.ContainsKey("Authorization"));
        }

        [Fact]
        public async Task InvokeAsync_ShouldNotRenewToken_WhenTokenIsInvalid()
        {
            // Arrange
            var mockTokenService = new Mock<IJWTService>();

            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = "Bearer invalidToken";

            var middleware = new TokenRenewalMiddleware(next: (innerHttpContext) => Task.CompletedTask, tokenService: mockTokenService.Object);

            // Mock the token handler
            var mockTokenHandler = new Mock<JsonWebTokenHandler>();
            mockTokenHandler.Setup(handler => handler.CanReadToken("invalidToken")).Returns(false);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.False(context.Response.Headers.ContainsKey("Authorization"));
        }

        [Fact]
        public async Task InvokeAsync_ShouldNotRenewToken_WhenNoTokenProvided()
        {
            // Arrange
            var mockTokenService = new Mock<IJWTService>();

            var context = new DefaultHttpContext();

            var middleware = new TokenRenewalMiddleware(next: (innerHttpContext) => Task.CompletedTask, tokenService: mockTokenService.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.False(context.Response.Headers.ContainsKey("Authorization"));
        }
    }
}