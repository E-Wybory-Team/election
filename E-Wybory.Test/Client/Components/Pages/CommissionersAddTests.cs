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
    public class CommissionersAddTests : TestContext
    {
        public CommissionersAddTests()
        {
            var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Role, "Administratorzy"),
            }, "test"))));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(authState));

            Services.AddAuthorizationCore();
            Services.AddSingleton<IAuthorizationPolicyProvider, DefaultAuthorizationPolicyProvider>();
            Services.AddSingleton<IAuthorizationService, DefaultAuthorizationService>();
        }

        [Fact]
        public void CommissionersAdd_Should_Render_Correctly_For_Authorized_User()
        {
            // Act
            var cut = RenderComponent<CommissionersAdd>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("DODAWANIE CZŁONKÓW KOMISJI DO OBWODU", cut.Markup);
                Assert.Contains("PESEL CZŁONKA KOMISJI", cut.Markup);
                Assert.Contains("WOJEWÓDZTWO", cut.Markup);
                Assert.Contains("POWIAT", cut.Markup);
                Assert.Contains("GMINA", cut.Markup);
                Assert.Contains("NUMER OBWODU", cut.Markup);
                Assert.Contains("STOPIEŃ", cut.Markup);
                Assert.Contains("DODAJ", cut.Markup);
                Assert.Contains("ANULUJ", cut.Markup);
            });
        }

        [Fact]
        public void CommissionersAdd_Should_Render_NotAuthorized_For_Unauthorized_User()
        {
            // Arrange
            var unauthorizedAuthState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(unauthorizedAuthState));

            // Act
            var cut = RenderComponent<CommissionersAdd>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nie posiadasz odpowiednich uprawnień do wyświetlenia tej strony", cut.Markup);
                Assert.Contains("WarningIcon.png", cut.Markup);
            });
        }

        [Fact]
        public void CommissionersAdd_Should_Display_Validation_Errors_On_Invalid_Form_Submission()
        {
            // Act
            var cut = RenderComponent<CommissionersAdd>();
            var submitButton = cut.Find("button.submit-button");
            submitButton.Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Numer PESEL jest wymagany.", cut.Markup);
                Assert.Contains("Województwo jest wymagane.", cut.Markup);
                Assert.Contains("Powiat jest wymagany.", cut.Markup);
                Assert.Contains("Gmina jest wymagana.", cut.Markup);
                Assert.Contains("Numer obwodu jest wymagany.", cut.Markup);
                Assert.Contains("Stopień członkostwa jest wymagany.", cut.Markup);
            });
        }

        [Fact]
        public void CommissionersAdd_Should_Navigate_To_CommissionersList_On_Cancel()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<CommissionersAdd>();
            var cancelButton = cut.Find("button.cancel-button");
            cancelButton.Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.EndsWith("/commissionerslist", navigationManager.Uri);
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
