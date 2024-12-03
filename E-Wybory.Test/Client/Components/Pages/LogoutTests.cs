using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using E_Wybory.Client.Services;
using System.Threading.Tasks;
using Bunit.TestDoubles;
using E_Wybory.Client.Components.Pages;
using Microsoft.AspNetCore.Components;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class LogoutTests : TestContext
    {
        private readonly Mock<IAuthService> _authServiceMock;

        public LogoutTests()
        {
            _authServiceMock = new Mock<IAuthService>();

            Services.AddSingleton(_authServiceMock.Object);

            var authState = Task.FromResult(new AuthenticationState(new System.Security.Claims.ClaimsPrincipal()));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(authState));

            Services.AddAuthorizationCore();
        }

        [Fact]
        public void Logout_Should_Render_Correctly()
        {
            // Act
            var cut = RenderComponent<Logout>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Czy na pewno chcesz się wylogować?", cut.Markup);
                Assert.Contains("Wyloguj się", cut.Markup);
                Assert.Contains("Anuluj", cut.Markup);
            });
        }

        [Fact]
        public async Task Logout_Should_Call_AuthService_Logout_On_Confirm()
        {
            // Arrange
            _authServiceMock.Setup(x => x.Logout()).ReturnsAsync(true);

            var cut = RenderComponent<Logout>();
            var logoutButton = cut.Find("button.submit-button");

            // Act
            await cut.InvokeAsync(() => logoutButton.Click());

            // Assert
            cut.WaitForAssertion(() =>
            {
                _authServiceMock.Verify(x => x.Logout(), Times.Once);
            });
        }

        [Fact]
        public void Logout_Should_Navigate_To_Home_On_Cancel()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<Logout>();
            var cancelButton = cut.Find("button.cancel-button");
            cancelButton.Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Equal(navigationManager.BaseUri, navigationManager.Uri);
            });
        }



        private class FakeAuthenticationStateProvider : AuthenticationStateProvider
        {
            private readonly Task<AuthenticationState> _authenticationState;

            public FakeAuthenticationStateProvider(Task<AuthenticationState> authenticationState)
            {
                _authenticationState = authenticationState;
            }

            public override Task<AuthenticationState> GetAuthenticationStateAsync()
            {
                return _authenticationState;
            }
        }
    }
}
