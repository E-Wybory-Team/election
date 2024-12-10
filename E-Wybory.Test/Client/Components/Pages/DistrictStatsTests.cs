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

namespace E_Wybory.Test.Client.Components.Pages
{
    public class DistrictStatsTests : TestContext
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IVoteManagementService> _voteManagementServiceMock;
        private readonly Mock<IVoterManagementService> _voterManagementServiceMock;
        private readonly Mock<IElectionManagementService> _electionManagementServiceMock;
        private readonly Mock<IElectionTypeManagementService> _electionTypeManagementServiceMock;
        private readonly Mock<IDistrictManagementService> _districtManagementServiceMock;

        public DistrictStatsTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _voteManagementServiceMock = new Mock<IVoteManagementService>();
            _voterManagementServiceMock = new Mock<IVoterManagementService>();
            _electionManagementServiceMock = new Mock<IElectionManagementService>();
            _electionTypeManagementServiceMock = new Mock<IElectionTypeManagementService>();
            _districtManagementServiceMock = new Mock<IDistrictManagementService>();

            _authServiceMock
                .Setup(service => service.GetCurrentUserIdDistrict())
                .ReturnsAsync(1);

            _voterManagementServiceMock
                .Setup(service => service.GetNumberVotersByDistrictId(It.IsAny<int>()))
                .ReturnsAsync(500);

            _electionManagementServiceMock
                .Setup(service => service.GetNewestElections())
                .ReturnsAsync(new List<ElectionViewModel>
                {
                    new ElectionViewModel { IdElection = 1, IdElectionType = 1, ElectionStartDate = DateTime.Now }
                });

            _districtManagementServiceMock
                .Setup(service => service.GetDistrictById(It.IsAny<int>()))
                .ReturnsAsync(new DistrictViewModel
                {
                    IdDistrict = 1,
                    DistrictHeadquarters = "Siedziba 1"
                });

            _voteManagementServiceMock
                .Setup(service => service.GetFrequencyByDistrictIdToHour(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(50.0);

            _electionTypeManagementServiceMock
                .Setup(service => service.GetElectionTypeName(It.IsAny<int>()))
                .ReturnsAsync("Wybory Testowe");

            Services.AddSingleton(_authServiceMock.Object);
            Services.AddSingleton(_voteManagementServiceMock.Object);
            Services.AddSingleton(_voterManagementServiceMock.Object);
            Services.AddSingleton(_electionManagementServiceMock.Object);
            Services.AddSingleton(_districtManagementServiceMock.Object);
            Services.AddSingleton(_electionTypeManagementServiceMock.Object);

            var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Role, "Komisja wyborcza"),
            }, "test"))));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(authState));

            Services.AddAuthorizationCore();
            Services.AddSingleton<IAuthorizationPolicyProvider, DefaultAuthorizationPolicyProvider>();
            Services.AddSingleton<IAuthorizationService, DefaultAuthorizationService>();
        }

        [Fact]
        public void DistrictStats_Should_Display_Error_When_No_District_Assigned()
        {
            // Arrange
            _authServiceMock
                .Setup(service => service.GetCurrentUserIdDistrict())
                .ThrowsAsync(new Exception("Nie znaleziono przypisanego obwodu użytkownikowi!"));

            // Act
            var cut = RenderComponent<DistrictStats>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nie znaleziono przypisanego obwodu użytkownikowi!", cut.Markup);
            });
        }

        [Fact]
        public void DistrictStats_Should_Render_Correctly_For_Authorized_User()
        {
            // Act
            var cut = RenderComponent<DistrictStats>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("STATYSTYKI GŁOSOWANIA DLA OBWODU", cut.Markup);
                Assert.Contains("Siedziba 1", cut.Markup);
                Assert.Contains("500", cut.Markup);
                Assert.Contains("50", cut.Markup); 
                Assert.Contains("Wybory Testowe", cut.Markup);
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
