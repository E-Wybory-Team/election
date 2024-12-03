using Bunit;
using Moq;
using Xunit;
using E_Wybory.Client.ViewModels;
using E_Wybory.Client.Components.Pages;
using E_Wybory.Client.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using E_Wybory.Client.FilterData;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class VoterDeleteTests : TestContext
    {
        private readonly Mock<IDistrictManagementService> _districtManagementServiceMock;
        private readonly Mock<IVoterManagementService> _voterManagementServiceMock;
        private readonly Mock<IPersonManagementService> _personManagementServiceMock;
        private readonly Mock<IElectionUserManagementService> _electionUserManagementServiceMock;

        public VoterDeleteTests()
        {
            _districtManagementServiceMock = new Mock<IDistrictManagementService>();
            _voterManagementServiceMock = new Mock<IVoterManagementService>();
            _personManagementServiceMock = new Mock<IPersonManagementService>();
            _electionUserManagementServiceMock = new Mock<IElectionUserManagementService>();

            Services.AddSingleton(_districtManagementServiceMock.Object);
            Services.AddSingleton(_voterManagementServiceMock.Object);
            Services.AddSingleton(_personManagementServiceMock.Object);
            Services.AddSingleton(_electionUserManagementServiceMock.Object);

            var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Role, "Urzędnicy wyborczy"),
            }, "test"))));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(authState));

            var authorizationPolicyProvider = new Mock<IAuthorizationPolicyProvider>();
            authorizationPolicyProvider.Setup(x => x.GetPolicyAsync(It.IsAny<string>()))
                .ReturnsAsync(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
            Services.AddSingleton(authorizationPolicyProvider.Object);

            var authorizationServiceMock = new Mock<IAuthorizationService>();
            authorizationServiceMock.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Success());
            Services.AddSingleton(authorizationServiceMock.Object);
        }

        [Fact]
        public void VoterDelete_Should_Display_Initial_Form()
        {
            // Act
            var cut = RenderComponent<VotersDelete>(parameters => parameters.Add(p => p.voterId, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("USUWANIE WYBORCY Z OBWODU", cut.Markup);
                Assert.Contains("PESEL WYBORCY", cut.Markup);
                Assert.Contains("NUMER OBWODU", cut.Markup);
            });
        }

        [Fact]
        public async Task VoterDelete_Should_Delete_Voter_With_Valid_Data()
        {
            // Arrange
            _voterManagementServiceMock
                .Setup(service => service.DeleteVoter(It.IsAny<int>()))
                .ReturnsAsync(true);

            _personManagementServiceMock
                .Setup(service => service.GetPersonIdByPeselAsync(It.IsAny<string>()))
                .ReturnsAsync(1);

            _electionUserManagementServiceMock
                .Setup(service => service.GetElectionUserByPersonId(1))
                .ReturnsAsync(new ElectionUserViewModel { IdElectionUser = 1 });

            _voterManagementServiceMock
                .Setup(service => service.GetVoterById(It.IsAny<int>()))
                .ReturnsAsync(new VoterViewModel { IdVoter = 1, IdElectionUser = 1, IdDistrict = 1 });

            var cut = RenderComponent<VotersDelete>(parameters => parameters.Add(p => p.voterId, 1));

            // Act
            var submitButton = cut.Find("button.red-submit-button");
            await cut.InvokeAsync(() => submitButton.Click());

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Updating voter successful!", cut.Markup);
            });
        }

        [Fact]
        public async Task VoterDelete_Should_Navigate_To_VotersList_On_Cancel()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<VotersDelete>(parameters => parameters.Add(p => p.voterId, 1));
            var cancelButton = cut.Find("button.cancel-button");
            cancelButton.Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.EndsWith("/voterslist", navigationManager.Uri);
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
