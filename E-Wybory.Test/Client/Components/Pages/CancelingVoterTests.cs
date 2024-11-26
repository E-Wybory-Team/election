using Bunit;
using Xunit;
using E_Wybory.Client.Components.Pages;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using E_Wybory.Client.Services;
using E_Wybory.Client.ViewModels;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class CancelingVoterTests : TestContext
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IElectionManagementService> _electionManagementServiceMock;
        private readonly Mock<IDistrictManagementService> _districtManagementServiceMock;
        private readonly Mock<IPersonManagementService> _personManagementServiceMock;
        private readonly Mock<IElectionUserManagementService> _electionUserManagementServiceMock;
        private readonly Mock<IVoterManagementService> _voterManagementServiceMock;

        public CancelingVoterTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _electionManagementServiceMock = new Mock<IElectionManagementService>();
            _districtManagementServiceMock = new Mock<IDistrictManagementService>();
            _personManagementServiceMock = new Mock<IPersonManagementService>();
            _electionUserManagementServiceMock = new Mock<IElectionUserManagementService>();
            _voterManagementServiceMock = new Mock<IVoterManagementService>();

            Services.AddSingleton(_authServiceMock.Object);
            Services.AddSingleton(_electionManagementServiceMock.Object);
            Services.AddSingleton(_districtManagementServiceMock.Object);
            Services.AddSingleton(_personManagementServiceMock.Object);
            Services.AddSingleton(_electionUserManagementServiceMock.Object);
            Services.AddSingleton(_voterManagementServiceMock.Object);

            // Add fake authentication state provider
            var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Administratorzy"),
            }, "test"))));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(authState));

            // Add authorization services
            Services.AddAuthorizationCore();
            Services.AddSingleton<IAuthorizationPolicyProvider, DefaultAuthorizationPolicyProvider>();
            Services.AddSingleton<IAuthorizationService, DefaultAuthorizationService>();
        }

        [Fact]
        public void CancelingVoter_Should_Render_Correctly()
        {
            // Arrange
            _authServiceMock.Setup(service => service.GetCurrentUserIdDistrict()).ReturnsAsync(1);
            _districtManagementServiceMock.Setup(service => service.GetDistrictById(It.IsAny<int>())).ReturnsAsync(new DistrictViewModel 
            { DistrictName = "District 1", 
              DistrictHeadquarters = "HQ 1",
              IdDistrict = 1,
              DisabledFacilities = false,
              IdConstituency = 1,
              IdProvince = 1
            });

            _electionManagementServiceMock.Setup(service => service.GetActiveElections()).ReturnsAsync(new List<ElectionViewModel>
            {
                new ElectionViewModel
                {
                    IdElection = 1,
                    ElectionStartDate = DateTime.Now,
                    ElectionEndDate = DateTime.Now.AddHours(1),
                    IdElectionType = 1,
                }
            });

            // Act
            var cut = RenderComponent<CancelingVoter>();

            // Assert
            cut.MarkupMatches(@"<link href=""forms.css"" rel=""stylesheet"">
                                <div class=""form-container"">
                                    <h1>WYPISYWANIE WYBORCY</h1>
                                    <form>
                                        <div class=""form-group"">
                                            <label for=""district"">OBWÓD WYBORCY</label>
                                            <input type=""district"" id=""pesel"" value=""District 1 - HQ 1"" class=""form-control"" placeholder=""Obwód wyborcy"" readonly>
                                        </div>
                                        <div class=""form-group"">
                                            <label for=""pesel"">PESEL WYBORCY</label>
                                            <input type=""text"" id=""pesel"" class=""form-control"" placeholder=""Nr Pesel wyborcy"">
                                        </div>
                                        <div class=""form-row"">
                                            <button type=""submit"" class=""submit-button"" >DALEJ</button>
                                            <button type=""button"" class=""cancel-button"" >ANULUJ</button>
                                        </div>
                                    </form>
                                </div>");
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