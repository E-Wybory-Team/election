/*using Bunit;
using Moq;
using Xunit;
using E_Wybory.Client.ViewModels;
using E_Wybory.Client.Components.Pages;
using E_Wybory.Client.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class VoteTests : TestContext
    {
        private readonly Mock<IVoteManagementService> _voteManagementServiceMock;
        private readonly Mock<IElectionManagementService> _electionManagementServiceMock;
        private readonly Mock<IElectionVoterManagementService> _electionVoterManagementServiceMock;
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<ICandidateManagementService> _candidateManagementServiceMock;
        private readonly Mock<IElectionTypeManagementService> _electionTypeManagementServiceMock;
        private readonly Mock<IPartyManagementService> _partyManagementServiceMock;
        private readonly Mock<IDistrictManagementService> _districtManagementServiceMock;
        private readonly Mock<IPersonManagementService> _personManagementServiceMock;
        private readonly Mock<IVoterManagementService> _voterManagementServiceMock;
        public VoteTests()
        {
            _voteManagementServiceMock = new Mock<IVoteManagementService>();
            _electionManagementServiceMock = new Mock<IElectionManagementService>();
            _electionVoterManagementServiceMock = new Mock<IElectionVoterManagementService>();
            _authServiceMock = new Mock<IAuthService>();
            _candidateManagementServiceMock = new Mock<ICandidateManagementService>();
            _electionTypeManagementServiceMock = new Mock<IElectionTypeManagementService>();
            _partyManagementServiceMock = new Mock<IPartyManagementService>();
            _districtManagementServiceMock = new Mock<IDistrictManagementService>();
            _personManagementServiceMock = new Mock<IPersonManagementService>();
            _voterManagementServiceMock = new Mock<IVoterManagementService>();

            Services.AddSingleton(_voteManagementServiceMock.Object);
            Services.AddSingleton(_electionManagementServiceMock.Object);
            Services.AddSingleton(_electionVoterManagementServiceMock.Object);
            Services.AddSingleton(_authServiceMock.Object);
            Services.AddSingleton(_candidateManagementServiceMock.Object);
            Services.AddSingleton(_electionTypeManagementServiceMock.Object);
            Services.AddSingleton(_partyManagementServiceMock.Object);
            Services.AddSingleton(_districtManagementServiceMock.Object);
            Services.AddSingleton(_personManagementServiceMock.Object);
            Services.AddSingleton(_voterManagementServiceMock.Object);


            var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(new[]
           {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Role, "Komisja wyborcza"),
            }, "test"))));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(authState));

            _electionTypeManagementServiceMock
                .Setup(service => service.ElectionTypes())
                .ReturnsAsync(new List<ElectionTypeViewModel>
                {
                    new ElectionTypeViewModel { IdElectionType = 1, ElectionTypeName = "Wybory Prezydenckie" },
                    new ElectionTypeViewModel { IdElectionType = 2, ElectionTypeName = "Wybory Parlamentarne" }
                });

            _partyManagementServiceMock
                .Setup(service => service.Parties())
                .ReturnsAsync(new List<PartyViewModel>
                {
                    new PartyViewModel { IdParty = 1, PartyName = "Partia A" },
                    new PartyViewModel { IdParty = 2, PartyName = "Partia B" }
                });
        }

        [Fact]
        public void Vote_Should_Display_Error_When_No_Elections_Available()
        {
            // Arrange
            _electionManagementServiceMock.Setup(service => service.GetActiveElections()).ReturnsAsync(new List<ElectionViewModel>());

            // Act
            var cut = RenderComponent<Vote>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nie ma żadnych trwających wyborów.", cut.Markup);
            });
        }

        [Fact]
        public async Task Vote_Should_Submit_ValidVote()
        {
            // Arrange
            _electionManagementServiceMock.Setup(service => service.GetActiveElections())
                .ReturnsAsync(new List<ElectionViewModel> { new ElectionViewModel { IdElection = 1 } });
            _candidateManagementServiceMock.Setup(service => service.GetCandidatesByElectionDistrictId(1, It.IsAny<int>()))
                .ReturnsAsync(new List<CandidateViewModel> { new CandidateViewModel { IdCandidate = 1 } });
            _voteManagementServiceMock.Setup(service => service.AddVote(It.IsAny<VoteViewModel>())).ReturnsAsync(true);
            _electionVoterManagementServiceMock.Setup(service => service.AddElectionVoter(It.IsAny<ElectionVoterViewModel>()))
                .ReturnsAsync(true);

            var cut = RenderComponent<Vote>();

            var candidateSelect = cut.Find("select#kandydat");
            candidateSelect.Change("1"); // Simulujemy wybór kandydata

            var submitButton = cut.Find("button[type='submit']");
            await cut.InvokeAsync(() => submitButton.Click());

            // Assert
            var markup = cut.Markup;
            Console.WriteLine(markup);

            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Głosowanie się powiodło!", cut.Markup);
            });
        }

        [Fact]
        public void Vote_Should_Display_Error_When_User_Already_Voted()
        {
            // Arrange
            _electionVoterManagementServiceMock.Setup(service => service.ElectionVoterExists(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);

            var cut = RenderComponent<Vote>();

            // Act
            var submitButton = cut.Find("button[type='submit']");
            submitButton.Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Użytkownik już głosował!", cut.Markup);
            });
        }

        [Fact]
        public async Task Vote_Should_Handle_InvalidVote()
        {
            // Arrange
            _electionManagementServiceMock.Setup(service => service.GetActiveElections())
                .ReturnsAsync(new List<ElectionViewModel> { new ElectionViewModel { IdElection = 1 } });
            _candidateManagementServiceMock.Setup(service => service.GetCandidatesByElectionDistrictId(1, It.IsAny<int>()))
                .ReturnsAsync(new List<CandidateViewModel> { new CandidateViewModel { IdCandidate = 1 } });
            _voteManagementServiceMock.Setup(service => service.AddVote(It.IsAny<VoteViewModel>())).ReturnsAsync(true);
            _electionVoterManagementServiceMock.Setup(service => service.AddElectionVoter(It.IsAny<ElectionVoterViewModel>()))
                .ReturnsAsync(true);

            var cut = RenderComponent<Vote>();

            var invalidVoteCheckbox = cut.Find("input#niewazny");
            await cut.InvokeAsync(() => invalidVoteCheckbox.Change(true));

            var submitButton = cut.Find("button[type='submit']");
            await cut.InvokeAsync(() => submitButton.Click());

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Głosowanie się powiodło!", cut.Markup);
            });
        }

        [Fact]
        public void Vote_Should_Display_Candidates_For_Election()
        {
            // Arrange
            _candidateManagementServiceMock.Setup(service => service.GetCandidatesByElectionDistrictId(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<CandidateViewModel>
                {
                    new CandidateViewModel { IdCandidate = 1 },
                    new CandidateViewModel { IdCandidate = 2 }
                });

            var cut = RenderComponent<Vote>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Wybierz kandydata", cut.Markup);
                Assert.Contains("1", cut.Markup);
                Assert.Contains("2", cut.Markup);
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
*/