using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using E_Wybory.Client.Services;
using System.Threading.Tasks;
using Bunit.TestDoubles;
using E_Wybory.Client.ViewModels;
using Microsoft.AspNetCore.Components;
using E_Wybory.Client.Components.Pages;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class LoginTests : TestContext
    {
        private readonly Mock<IAuthService> _authServiceMock;

        public LoginTests()
        {
            _authServiceMock = new Mock<IAuthService>();

            Services.AddSingleton(_authServiceMock.Object);

            var authState = Task.FromResult(new AuthenticationState(new System.Security.Claims.ClaimsPrincipal()));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(authState));

            Services.AddAuthorizationCore();
        }

        [Fact]
        public void Login_Should_Render_Correctly()
        {
            // Act
            var cut = RenderComponent<Login>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("ZALOGUJ", cut.Markup);
                Assert.Contains("Nie pamiętasz hasła?", cut.Markup);
                Assert.Contains("Nie masz konta?", cut.Markup);
            });
        }

        [Fact]
        public void Login_Should_Display_Validation_Errors_For_Empty_Fields()
        {
            // Act
            var cut = RenderComponent<Login>();
            var form = cut.Find("form");
            form.Submit();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nazwa użytkownika jest wymagana.", cut.Markup);
                Assert.Contains("Hasło jest wymagane.", cut.Markup);
            });
        }

        [Fact]
        public async Task Login_Should_Display_Error_For_Invalid_Credentials()
        {
            // Arrange
            _authServiceMock.Setup(x => x.Login(It.IsAny<LoginViewModel>())).ReturnsAsync(string.Empty);

            var cut = RenderComponent<Login>();
            var usernameInput = cut.Find("input[placeholder='E-MAIL']");
            var passwordInput = cut.Find("input[placeholder='HASŁO']");

            // Act
            await cut.InvokeAsync(() => usernameInput.Change("invalid@example.com"));
            await cut.InvokeAsync(() => passwordInput.Change("wrongPassword"));
            var form = cut.Find("form");
            form.Submit();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Invalid useasdrname or password!", cut.Markup);
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

