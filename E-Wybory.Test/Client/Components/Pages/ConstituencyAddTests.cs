using Bunit;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Moq;
using E_Wybory.Client.ViewModels;
using E_Wybory.Client.Components.Pages;
using E_Wybory.Client.Services;
using Microsoft.AspNetCore.Components;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class ConstituencyAddTests : TestContext
    {
        private readonly Mock<IConstituencyManagementService> _constituencyManagementServiceMock;

        public ConstituencyAddTests()
        {
            _constituencyManagementServiceMock = new Mock<IConstituencyManagementService>();
            _constituencyManagementServiceMock.Setup(service => service.AddConstituency(It.IsAny<ConstituencyViewModel>())).ReturnsAsync(true);

            Services.AddSingleton(_constituencyManagementServiceMock.Object);

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
        public void ConstituencyAdd_Should_Render_Correctly_For_Authorized_User()
        {
            // Act
            var cut = RenderComponent<ConstituencyAdd>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("DODAWANIE OKRĘGU WYBORCZEGO", cut.Markup);
                Assert.Contains("Nazwa okręgu wyborczego", cut.Markup);
                Assert.Contains("DODAJ", cut.Markup);
                Assert.Contains("ANULUJ", cut.Markup);
            });
        }

        [Fact]
        public void ConstituencyAdd_Should_Render_NotAuthorized_For_Unauthorized_User()
        {
            // Arrange
            var unauthorizedAuthState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(unauthorizedAuthState));

            // Act
            var cut = RenderComponent<ConstituencyAdd>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nie posiadasz odpowiednich uprawnień do wyświetlenia tej strony", cut.Markup);
                Assert.Contains("WarningIcon.png", cut.Markup);
            });
        }

        [Fact]
        public void ConstituencyAdd_Should_Call_Service_On_ValidSubmission()
        {
            // Act
            var cut = RenderComponent<ConstituencyAdd>();
            var inputField = cut.Find("input#constituencyName");
            inputField.Change("Testowy Okręg");
            var submitButton = cut.Find("button.submit-button");
            submitButton.Click();

            // Assert
            _constituencyManagementServiceMock.Verify(service => service.AddConstituency(It.Is<ConstituencyViewModel>(
                model => model.constituencyName == "Testowy Okręg")), Times.Once);
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Adding constituency successful!", cut.Markup);
            });
        }

        [Fact]
        public void ConstituencyAdd_Should_Navigate_To_ConstituencyList_On_Cancel()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<ConstituencyAdd>();
            var cancelButton = cut.Find("button.cancel-button");
            cancelButton.Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.EndsWith("/constituencylist", navigationManager.Uri);
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
