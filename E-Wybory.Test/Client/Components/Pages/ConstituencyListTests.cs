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
using System.Collections.Generic;
using E_Wybory.Client.Services;
using Microsoft.AspNetCore.Components;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class ConstituencyListTests : TestContext
    {
        private readonly Mock<IConstituencyManagementService> _constituencyManagementServiceMock;
        private readonly Mock<IFilterWrapperManagementService> _filterWrapperServiceMock;

        public ConstituencyListTests()
        {
            _constituencyManagementServiceMock = new Mock<IConstituencyManagementService>();

            _constituencyManagementServiceMock
                .Setup(service => service.Constituences())
                .ReturnsAsync(new List<ConstituencyViewModel>
                {
                    new ConstituencyViewModel { idConstituency = 1, constituencyName = "Okręg 1" },
                    new ConstituencyViewModel { idConstituency = 2, constituencyName = "Okręg 2" }
                });

            _constituencyManagementServiceMock
                .Setup(service => service.GetCountiesOfConstituency(1))
                .ReturnsAsync(new List<CountyViewModel>
                {
                    new CountyViewModel { CountyName = "Powiat 1" },
                    new CountyViewModel { CountyName = "Powiat 2" }
                });

            _constituencyManagementServiceMock
                .Setup(service => service.GetCountiesOfConstituency(2))
                .ReturnsAsync(new List<CountyViewModel>
                {
                    new CountyViewModel { CountyName = "Powiat 3" }
                });

            Services.AddSingleton(_constituencyManagementServiceMock.Object);

            _filterWrapperServiceMock = new Mock<IFilterWrapperManagementService>();

            Services.AddSingleton(_filterWrapperServiceMock.Object);

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
        public void ConstituencyList_Should_Render_Correctly_For_Authorized_User()
        {
            // Act
            var cut = RenderComponent<ConstituencyList>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("KONFIGURACJA OKRĘGÓW WYBORCZYCH", cut.Markup);
                Assert.Contains("Okręg 1", cut.Markup);
                Assert.Contains("Okręg 2", cut.Markup);
                Assert.Contains("Powiat 1", cut.Markup);
                Assert.Contains("Powiat 2", cut.Markup);
                Assert.Contains("Powiat 3", cut.Markup);
                Assert.Contains("Dodaj okręg", cut.Markup);
            });
        }

        [Fact]
        public void ConstituencyList_Should_Render_NotAuthorized_For_Unauthorized_User()
        {
            // Arrange
            var unauthorizedAuthState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(unauthorizedAuthState));

            // Act
            var cut = RenderComponent<ConstituencyList>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nie posiadasz odpowiednich uprawnień do wyświetlenia tej strony", cut.Markup);
                Assert.Contains("WarningIcon.png", cut.Markup);
            });
        }

        [Fact]
        public void ConstituencyList_Should_Call_NavigateTo_AddConstituency_On_ButtonClick()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<ConstituencyList>();
            var addButton = cut.Find("button.add-button");
            addButton.Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.EndsWith("/addconstituency", navigationManager.Uri);
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
