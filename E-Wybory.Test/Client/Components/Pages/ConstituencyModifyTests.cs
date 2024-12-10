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
    public class ConstituencyModifyTests : TestContext
    {
        private readonly Mock<IConstituencyManagementService> _constituencyManagementServiceMock;

        public ConstituencyModifyTests()
        {
            _constituencyManagementServiceMock = new Mock<IConstituencyManagementService>();

            _constituencyManagementServiceMock
                .Setup(service => service.ConstituencyExists(It.IsAny<int>()))
                .ReturnsAsync(true);

            _constituencyManagementServiceMock
                .Setup(service => service.GetConstituencyById(It.IsAny<int>()))
                .ReturnsAsync(new ConstituencyViewModel
                {
                    idConstituency = 1,
                    constituencyName = "Okręg 1"
                });

            _constituencyManagementServiceMock
                .Setup(service => service.PutConstituency(It.IsAny<ConstituencyViewModel>()))
                .ReturnsAsync(true);

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
        public void ConstituencyModify_Should_Render_Correctly_For_Authorized_User()
        {
            // Act
            var cut = RenderComponent<ConstituencyModify>(parameters => parameters.Add(p => p.constituencyId, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Console.WriteLine(cut.Markup);
                Assert.Contains("MODYFIKOWANIE OKRĘGU WYBORCZEGO", cut.Markup);
                Assert.Contains("NAZWA OKRĘGU", cut.Markup);
                Assert.Contains("Okręg 1", cut.Markup);
                Assert.Contains("ZAPISZ ZMIANY", cut.Markup);
                Assert.Contains("ANULUJ", cut.Markup);
            });
        }

        [Fact]
        public void ConstituencyModify_Should_Render_NotAuthorized_For_Unauthorized_User()
        {
            // Arrange
            var unauthorizedAuthState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(unauthorizedAuthState));

            // Act
            var cut = RenderComponent<ConstituencyModify>(parameters => parameters.Add(p => p.constituencyId, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nie posiadasz odpowiednich uprawnień do wyświetlenia tej strony", cut.Markup);
                Assert.Contains("WarningIcon.png", cut.Markup);
            });
        }

        [Fact]
        public void ConstituencyModify_Should_Navigate_To_ConstituencyList_On_Cancel()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<ConstituencyModify>(parameters => parameters.Add(p => p.constituencyId, 1));
            var cancelButton = cut.Find("button.cancel-button");
            cancelButton.Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.EndsWith("/constituencylist", navigationManager.Uri);
            });
        }

        [Fact]
        public void ConstituencyModify_Should_Call_Service_On_ValidSubmission()
        {
            // Arrange
            var cut = RenderComponent<ConstituencyModify>(parameters => parameters.Add(p => p.constituencyId, 1));
            var form = cut.Find("form");

            // Act
            form.Submit();

            // Assert
            _constituencyManagementServiceMock.Verify(service => service.PutConstituency(It.IsAny<ConstituencyViewModel>()), Times.Once);
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
