using Bunit;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using E_Wybory.Client.Components.Pages;
using Microsoft.AspNetCore.Components;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class ElectionConfigureTests : TestContext
    {
        public ElectionConfigureTests()
        {
            var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Role, "Pracownicy PKW"),
            }, "test"))));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(authState));

            Services.AddAuthorizationCore();
            Services.AddSingleton<IAuthorizationPolicyProvider, DefaultAuthorizationPolicyProvider>();
            Services.AddSingleton<IAuthorizationService, DefaultAuthorizationService>();
        }

        [Fact]
        public void ElectionConfigure_Should_Render_Correctly_For_Authorized_User()
        {
            // Act
            var cut = RenderComponent<ElectionConfigure>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("KONFIGURACJA WYBORÓW", cut.Markup);
                Assert.Contains("ZAREJESTRUJ WYBORY", cut.Markup);
                Assert.Contains("MODYFIKUJ WYBORY", cut.Markup);
                Assert.Contains("USUŃ WYBORY", cut.Markup);
            });
        }

        [Fact]
        public void ElectionConfigure_Should_Render_NotAuthorized_For_Unauthorized_User()
        {
            // Arrange
            var unauthorizedAuthState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(unauthorizedAuthState));

            // Act
            var cut = RenderComponent<ElectionConfigure>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nie posiadasz odpowiednich uprawnień do wyświetlenia tej strony", cut.Markup);
                Assert.Contains("WarningIcon.png", cut.Markup);
            });
        }

        [Fact]
        public void ElectionConfigure_Should_Navigate_To_AddElection_On_AddButton_Click()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<ElectionConfigure>();
            var addButton = cut.Find(".config-button-green");
            addButton.Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.EndsWith("/add-election", navigationManager.Uri);
            });
        }

        [Fact]
        public void ElectionConfigure_Should_Navigate_To_ModifyElection_On_ModifyButton_Click()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<ElectionConfigure>();
            var modifyButton = cut.Find(".config-button-blue");
            modifyButton.Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.EndsWith("/modify-election", navigationManager.Uri);
            });
        }

        [Fact]
        public void ElectionConfigure_Should_Navigate_To_DeleteElection_On_DeleteButton_Click()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<ElectionConfigure>();
            var deleteButton = cut.Find(".config-button-red");
            deleteButton.Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.EndsWith("/delete-election", navigationManager.Uri);
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
