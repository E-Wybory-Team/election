using E_Wybory.Middleware;
using E_Wybory.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;
using NuGet.Common;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace E_Wybory.Test.Server.Middleware
{
    public class TokenRenewalMiddlewareTests
    {
        private static readonly string _mockToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

        private JsonWebToken CreateMockJsonWebToken(string token, DateTime validFrom, DateTime validTo)
        {
            var mockTokenHandler = new Mock<JsonWebTokenHandler>();
            mockTokenHandler.Setup(handler => handler.CanReadToken(token)).Returns(true);
            mockTokenHandler.Setup(handler => handler.ReadJsonWebToken(token))
                .Returns(new JsonWebToken(token));

            var jsonWebToken = mockTokenHandler.Object.ReadJsonWebToken(token);
            SetTokenValidity(jsonWebToken, validFrom, validTo);

            return jsonWebToken;
        }

        private void SetTokenValidity(JsonWebToken token, DateTime validFrom, DateTime validTo)
        {
            var validFromField = typeof(JsonWebToken).GetField("_validFrom", BindingFlags.NonPublic | BindingFlags.Instance);
            var validToField = typeof(JsonWebToken).GetField("_validTo", BindingFlags.NonPublic | BindingFlags.Instance);

            validFromField.SetValue(token, validFrom);
            validToField.SetValue(token, validTo);
        }

        [Fact]
        public async Task InvokeAsync_ShouldRenewToken_WhenTokenIsValidAndNearExpiration()
        {
            // Arrange
            var mockTokenService = new Mock<IJWTService>();
            mockTokenService.Setup(service => service.RenewToken(It.IsAny<JsonWebToken>()))
                .ReturnsAsync("newToken");

            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = "Bearer validToken";

            var token = CreateMockJsonWebToken(_mockToken, DateTime.UtcNow.AddMinutes(-7), DateTime.UtcNow.AddMinutes(1));

            var mockTokenHandler = new Mock<JsonWebTokenHandler>();
            mockTokenHandler.Setup(handler => handler.CanReadToken("validToken")).Returns(true);
            mockTokenHandler.Setup(handler => handler.ReadJsonWebToken("validToken")).Returns(token);

            var middleware = new TokenRenewalMiddleware(next: (innerHttpContext) => Task.CompletedTask, tokenService: mockTokenService.Object,
                handler: mockTokenHandler.Object);

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

            var token = CreateMockJsonWebToken(_mockToken, DateTime.UtcNow.AddMinutes(-5), DateTime.UtcNow.AddMinutes(1));

            var mockTokenHandler = new Mock<JsonWebTokenHandler>();
            mockTokenHandler.Setup(handler => handler.CanReadToken("validToken")).Returns(true);
            mockTokenHandler.Setup(handler => handler.ReadJsonWebToken("validToken")).Returns(token);

            var middleware = new TokenRenewalMiddleware(next: (innerHttpContext) => Task.CompletedTask, tokenService: mockTokenService.Object,
                handler: mockTokenHandler.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.False(context.Response.Headers.ContainsKey("Authorization"));
        }

        [Fact]
        public async Task InvokeAsync_ShouldNotRenewToken_WhenTokenHasInvalidStructure()
        {
            // Arrange
            var mockTokenService = new Mock<IJWTService>();

            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = "Bearer invalidToken";

            var mockTokenHandler = new Mock<JsonWebTokenHandler>();
            mockTokenHandler.Setup(handler => handler.CanReadToken("invalidToken")).Returns(false);

            var middleware = new TokenRenewalMiddleware(next: (innerHttpContext) => Task.CompletedTask, tokenService: mockTokenService.Object,
                handler: mockTokenHandler.Object);

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

            var middleware = new TokenRenewalMiddleware(next: (innerHttpContext) => Task.CompletedTask, tokenService: mockTokenService.Object,
                handler: new JsonWebTokenHandler());

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.False(context.Response.Headers.ContainsKey("Authorization"));
        }
    }
}