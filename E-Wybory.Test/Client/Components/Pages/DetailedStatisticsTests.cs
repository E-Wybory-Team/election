/*using Bunit;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using E_Wybory.Client.Components.Pages;
using E_Wybory.Client.FilterData;
using E_Wybory.Client.ViewModels;
using Moq;
using System.Collections.Generic;
using E_Wybory.Client.Services;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class DetailedStatisticsTests : TestContext
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<ICandidateManagementService> _candidateServiceMock;
        private readonly Mock<IFilterWrapperManagementService> _filterWrapperServiceMock;
        private readonly Mock<IVoteManagementService> _voteManagementServiceMock;
        private readonly Mock<IElectionTypeManagementService> _electionTypeServiceMock;
        private readonly Mock<IPartyManagementService> _partyManagementServiceMock;
        private readonly Mock<IVoterManagementService> _voterManagementServiceMock;
        private readonly Mock<IElectionManagementService> _electionManagementServiceMock;
        private readonly Mock<IPersonManagementService> _personManagementServiceMock;

        public DetailedStatisticsTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _candidateServiceMock = new Mock<ICandidateManagementService>();
            _filterWrapperServiceMock = new Mock<IFilterWrapperManagementService>();
            _voteManagementServiceMock = new Mock<IVoteManagementService>();
            _electionTypeServiceMock = new Mock<IElectionTypeManagementService>();
            _partyManagementServiceMock = new Mock<IPartyManagementService>();
            _voterManagementServiceMock = new Mock<IVoterManagementService>();
            _electionManagementServiceMock = new Mock<IElectionManagementService>();
            _personManagementServiceMock = new Mock<IPersonManagementService>();

            // Rejestrowanie usług
            Services.AddSingleton(_authServiceMock.Object);
            Services.AddSingleton(_candidateServiceMock.Object);
            Services.AddSingleton(_filterWrapperServiceMock.Object);
            Services.AddSingleton(_voteManagementServiceMock.Object);
            Services.AddSingleton(_electionTypeServiceMock.Object);
            Services.AddSingleton(_partyManagementServiceMock.Object);
            Services.AddSingleton(_voterManagementServiceMock.Object);
            Services.AddSingleton(_electionManagementServiceMock.Object);
            Services.AddSingleton(_personManagementServiceMock.Object);

            // Dodanie fałszywego providera autoryzacji
            var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Role, "Administratorzy"),
            }, "test"))));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(authState));
        }

        [Fact]
        public void DetailedStatistics_Should_Render_Correctly()
        {
            // Arrange
            _authServiceMock.Setup(service => service.GetCurrentUserIdDistrict()).ReturnsAsync(1);
            _filterWrapperServiceMock.Setup(service => service.GetFilteredCandidatesFromElection(null, 1))
                .ReturnsAsync(new List<CandidatePersonViewModel>
                {
                    new CandidatePersonViewModel
                    {
                        candidateViewModel = new CandidateViewModel { IdCandidate = 1, PositionNumber = 1 },
                        personViewModel = new PersonViewModel { Name = "Jan", Surname = "Kowalski" }
                    }
                });

            // Act
            var cut = RenderComponent<DetailedStatistics>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("SZCZEGÓŁOWE WYNIKI DLA OBWODU", cut.Markup);
                Assert.Contains("Liczba uprawnionych wyborców:", cut.Markup);
            });
        }

        [Fact]
        public void DetailedStatistics_Should_Show_Error_If_No_District_Found()
        {
            // Arrange
            _authServiceMock.Setup(service => service.GetCurrentUserIdDistrict()).Throws(new Exception("No district found"));

            // Act
            var cut = RenderComponent<DetailedStatistics>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nie znaleziono przypisanego obwodu użytkownikowi!", cut.Markup);
            });
        }

        [Fact]
        public void DetailedStatistics_Should_Filter_By_ElectionType()
        {
            // Arrange
            _authServiceMock.Setup(service => service.GetCurrentUserIdDistrict()).ReturnsAsync(1);
            _filterWrapperServiceMock.Setup(service => service.GetFilteredCandidatesFromElection(1, 1))
                .ReturnsAsync(new List<CandidatePersonViewModel>
                {
                    new CandidatePersonViewModel
                    {
                        candidateViewModel = new CandidateViewModel { IdCandidate = 1, PositionNumber = 1 },
                        personViewModel = new PersonViewModel { Name = "Anna", Surname = "Nowak" }
                    }
                });

            _electionManagementServiceMock.Setup(service => service.GetNewestElectionOfElectionType(1))
                .ReturnsAsync(new ElectionViewModel { IdElection = 1 });

            // Act
            var cut = RenderComponent<DetailedStatistics>();
            cut.SetParametersAndRender(parameters => parameters.Add(p => p.SelectedElectionTypeId, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Anna Nowak", cut.Markup);
            });
        }

        private class FakeAuthenticationStateProvider : AuthenticationStateProvider
        {
            private readonly Task<AuthenticationState> _authenticationState;

            public FakeAuthenticationStateProvider(Task<AuthenticationState> authenticationState)
            {
                _authenticationState = authenticationState;
            }

            public override Task<AuthenticationState> GetAuthenticationStateAsync() => _authenticationState;
        }
    }
}
*/