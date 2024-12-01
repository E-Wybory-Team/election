using Bunit;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Moq;
using E_Wybory.Client.Components.Pages;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class CheckVoteStatusTests : TestContext
    {
        public CheckVoteStatusTests()
        {
            // Add fake authentication state provider
            var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Role, "Komisja wyborcza"),
            }, "test"))));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(authState));

            // Add authorization services
            Services.AddAuthorizationCore();
            Services.AddSingleton<IAuthorizationPolicyProvider, DefaultAuthorizationPolicyProvider>();
            Services.AddSingleton<IAuthorizationService, DefaultAuthorizationService>();

            // Remove the custom NavigationManager registration
            // Services.AddSingleton<NavigationManager, FakeNavigationManager>();
        }

        [Fact]
        public void CheckVoteStatus_Should_Render_Correctly_For_Authorized_User()
        {
            // Act
            var cut = RenderComponent<CheckVoteStatus>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("SPRAWDŹ STATUS GŁOSOWANIA", cut.Markup);
                Assert.Contains("OBWÓD GŁOSOWANIA", cut.Markup);
                Assert.Contains("OKW 1 Kędzierzyn Koźle", cut.Markup);
                Assert.Contains("PESEL WYBORCY", cut.Markup);
                Assert.Contains("SPRAWDŹ STATUS", cut.Markup);
                Assert.Contains("ANULUJ", cut.Markup);
            });
        }

        [Fact]
        public void CheckVoteStatus_Should_Render_NotAuthorized_For_Unauthorized_User()
        {
            // Arrange: Configure a user without the required role
            var unauthorizedAuthState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(unauthorizedAuthState));

            // Act
            var cut = RenderComponent<CheckVoteStatus>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nie posiadasz odpowiednich uprawnień do wyświetlenia tej strony", cut.Markup);
                Assert.Contains("WarningIcon.png", cut.Markup);
            });
        }

        [Fact]
        public void CheckVoteStatus_Should_Navigate_To_CheckStatusResult_On_CheckStatus()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<CheckVoteStatus>();
            var checkStatusButton = cut.Find("button.submit-button");

            Assert.Contains("SPRAWDŹ STATUS GŁOSOWANIA", cut.Markup); 
            checkStatusButton.Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Console.WriteLine($"Current URI: {navigationManager.Uri}");
                Assert.EndsWith("/checkstatusresult", navigationManager.Uri);
            });
        }

        [Fact]
        public void CheckVoteStatus_Should_Navigate_To_CoHome_On_Cancel()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<CheckVoteStatus>();
            var cancelButton = cut.Find("button.cancel-button");
            cancelButton.Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.EndsWith("/cohome", navigationManager.Uri);
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
