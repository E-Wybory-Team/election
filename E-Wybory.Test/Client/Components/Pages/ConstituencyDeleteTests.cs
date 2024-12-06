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
    public class ConstituencyDeleteTests : TestContext
    {
        private readonly Mock<IConstituencyManagementService> _constituencyManagementServiceMock;
        private readonly Mock<ICountyManagementService> _countyManagementServiceMock;

        public ConstituencyDeleteTests()
        {
            _constituencyManagementServiceMock = new Mock<IConstituencyManagementService>();
            _constituencyManagementServiceMock.Setup(service => service.ConstituencyExists(It.IsAny<int>())).ReturnsAsync(true);
            _constituencyManagementServiceMock.Setup(service => service.GetConstituencyById(It.IsAny<int>())).ReturnsAsync(new ConstituencyViewModel
            {
                constituencyName = "Test Okręg"
            });
            _constituencyManagementServiceMock.Setup(service => service.GetCountiesOfConstituency(It.IsAny<int>())).ReturnsAsync(new List<CountyViewModel>
            {
                new CountyViewModel { CountyName = "Powiat 1" },
                new CountyViewModel { CountyName = "Powiat 2" }
            });
            _constituencyManagementServiceMock.Setup(service => service.DeleteConstituency(It.IsAny<int>())).ReturnsAsync(true);

            _countyManagementServiceMock = new Mock<ICountyManagementService>();

            Services.AddSingleton(_constituencyManagementServiceMock.Object);
            Services.AddSingleton(_countyManagementServiceMock.Object);

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
        public void ConstituencyDelete_Should_Render_Correctly_For_Authorized_User()
        {
            // Act
            var cut = RenderComponent<ConstituencyDelete>(parameters => parameters.Add(p => p.constituencyId, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("POTWIERDŹ USUNIĘCIE OKRĘGU WYBORCZEGO", cut.Markup);
                Assert.Contains("NAZWA OKRĘGU", cut.Markup);
                Assert.Contains("Test Okręg", cut.Markup);
                Assert.Contains("ZASIĘG (POWIATY)", cut.Markup);
                Assert.Contains("Powiat 1", cut.Markup);
                Assert.Contains("Powiat 2", cut.Markup);
                Assert.Contains("USUŃ", cut.Markup);
                Assert.Contains("ANULUJ", cut.Markup);
            });
        }

        [Fact]
        public void ConstituencyDelete_Should_Render_NotAuthorized_For_Unauthorized_User()
        {
            // Arrange
            var unauthorizedAuthState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(unauthorizedAuthState));

            // Act
            var cut = RenderComponent<ConstituencyDelete>(parameters => parameters.Add(p => p.constituencyId, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nie posiadasz odpowiednich uprawnień do wyświetlenia tej strony", cut.Markup);
                Assert.Contains("WarningIcon.png", cut.Markup);
            });
        }

        [Fact]
        public void ConstituencyDelete_Should_Call_Service_On_ValidSubmission()
        {
            // Act
            var cut = RenderComponent<ConstituencyDelete>(parameters => parameters.Add(p => p.constituencyId, 1));
            var deleteButton = cut.Find("button.red-submit-button");
            deleteButton.Click();

            // Assert
            _constituencyManagementServiceMock.Verify(service => service.DeleteConstituency(1), Times.Once);
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Usunięto okręg pomyślnie!", cut.Markup);
            });
        }

        [Fact]
        public void ConstituencyDelete_Should_Navigate_To_ConstituencyList_On_Cancel()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<ConstituencyDelete>(parameters => parameters.Add(p => p.constituencyId, 1));
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
