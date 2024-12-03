using Bunit;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using Moq;
using E_Wybory.Client.ViewModels;
using E_Wybory.Client.Components.Pages;
using E_Wybory.Client.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class ElectionAddTests : TestContext
    {
        private readonly Mock<IElectionManagementService> _electionManagementServiceMock;
        private readonly Mock<IElectionTypeManagementService> _electionTypeManagementServiceMock;

        public ElectionAddTests()
        {
            _electionManagementServiceMock = new Mock<IElectionManagementService>();
            _electionTypeManagementServiceMock = new Mock<IElectionTypeManagementService>();

            _electionTypeManagementServiceMock
                .Setup(service => service.ElectionTypes())
                .ReturnsAsync(new List<ElectionTypeViewModel>
                {
                    new ElectionTypeViewModel { IdElectionType = 1, ElectionTypeName = "Wybory Prezydenckie" },
                    new ElectionTypeViewModel { IdElectionType = 2, ElectionTypeName = "Wybory Parlamentarne" }
                });

            _electionManagementServiceMock
                .Setup(service => service.AddElection(It.IsAny<ElectionViewModel>()))
                .ReturnsAsync(true);

            Services.AddSingleton(_electionManagementServiceMock.Object);
            Services.AddSingleton(_electionTypeManagementServiceMock.Object);

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
        public void ElectionAdd_Should_Render_Correctly_For_Authorized_User()
        {
            // Act
            var cut = RenderComponent<ElectionAdd>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("REJESTROWANIE WYBORÓW", cut.Markup);
                Assert.Contains("Wybory Prezydenckie", cut.Markup);
                Assert.Contains("Wybory Parlamentarne", cut.Markup);
                Assert.Contains("DODAJ", cut.Markup);
                Assert.Contains("ANULUJ", cut.Markup);
            });
        }

        [Fact]
        public void ElectionAdd_Should_Display_Error_For_Invalid_Dates()
        {
            // Act
            var cut = RenderComponent<ElectionAdd>();
            cut.Find("#dzien-wyborow").Change("2024-12-10T10:00");
            cut.Find("#dzien-wyborow-koniec").Change("2024-12-09T10:00");
            cut.Find("button.submit-button").Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Data zakończenia wyborów nie może być wcześniejsza niż data rozpoczęcia wyborów", cut.Markup);
            });
        }

        [Fact]
        public void ElectionAdd_Should_Call_Service_On_ValidSubmission()
        {
            // Act
            var cut = RenderComponent<ElectionAdd>();
            cut.Find("#dzien-wyborow").Change("2024-12-10T10:00");
            cut.Find("#dzien-wyborow-koniec").Change("2024-12-12T10:00");
            cut.Find("button.submit-button").Click();

            // Assert
            _electionManagementServiceMock.Verify(service => service.AddElection(It.IsAny<ElectionViewModel>()), Times.Once);
        }

        [Fact]
        public void ElectionAdd_Should_Navigate_To_ConfigureElection_On_Cancel()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<ElectionAdd>();
            cut.Find("button.cancel-button").Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.EndsWith("/configure-election", navigationManager.Uri);
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
