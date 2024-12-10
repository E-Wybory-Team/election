using Bunit;
using Moq;
using Xunit;
using E_Wybory.Client.Components.Pages;
using E_Wybory.Client.ViewModels;
using E_Wybory.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class CandidateDeleteTests : TestContext
    {
        private readonly Mock<ICandidateManagementService> _candidateManagementServiceMock;
        private readonly Mock<IPersonManagementService> _personManagementServiceMock;
        private readonly Mock<IDistrictManagementService> _districtManagementServiceMock;
        private readonly Mock<IPartyManagementService> _partyManagementServiceMock;
        private readonly Mock<IElectionManagementService> _electionManagementServiceMock;

        public CandidateDeleteTests()
        {
            _candidateManagementServiceMock = new Mock<ICandidateManagementService>();
            _personManagementServiceMock = new Mock<IPersonManagementService>();
            _districtManagementServiceMock = new Mock<IDistrictManagementService>();
            _partyManagementServiceMock = new Mock<IPartyManagementService>();
            _electionManagementServiceMock = new Mock<IElectionManagementService>();

            _districtManagementServiceMock.Setup(s => s.Districts()).ReturnsAsync(new List<DistrictViewModel>
            {
                new DistrictViewModel { IdDistrict = 1, DistrictName = "Obwód 1", DistrictHeadquarters = "Siedziba 1" }
            });
            _partyManagementServiceMock.Setup(s => s.Parties()).ReturnsAsync(new List<PartyViewModel>
            {
                new PartyViewModel { IdParty = 1, PartyName = "Partia A" }
            });
            _electionManagementServiceMock
               .Setup(service => service.Elections())
               .ReturnsAsync(new List<ElectionViewModel>
               {
                    new ElectionViewModel { IdElection = 1, IdElectionType = 1, ElectionTour = 1, ElectionStartDate = new DateTime(2023, 1, 1), ElectionEndDate = new DateTime(2023, 1, 2) },
               });

            var candidateId = 1;

            _candidateManagementServiceMock.Setup(s => s.CandidateExists(candidateId)).ReturnsAsync(true);
            _candidateManagementServiceMock.Setup(s => s.GetCandidateById(candidateId)).ReturnsAsync(new CandidateViewModel
            {
                IdCandidate = 1,
                IdPerson = 1,
                JobType = "Inżynier",
                PositionNumber = 2,
                PlaceOfResidence = "Warszawa"
            });
            _personManagementServiceMock.Setup(s => s.GetPersonById(1)).ReturnsAsync(new PersonViewModel
            {
                Name = "Jan",
                Surname = "Kowalski",
                PESEL = "12345678901",
                DateOfBirthString = "1990-01-01"
            });

            Services.AddSingleton(_candidateManagementServiceMock.Object);
            Services.AddSingleton(_personManagementServiceMock.Object);
            Services.AddSingleton(_districtManagementServiceMock.Object);
            Services.AddSingleton(_partyManagementServiceMock.Object);
            Services.AddSingleton(_electionManagementServiceMock.Object);

            var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Role, "Administratorzy"),
            }, "test"))));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(authState));

            Services.AddAuthorizationCore();
            Services.AddSingleton<IAuthorizationPolicyProvider, DefaultAuthorizationPolicyProvider>();
            Services.AddSingleton<IAuthorizationService, DefaultAuthorizationService>();

        }

        [Fact]
        public async Task CandidateDelete_Should_Render_Correctly_For_Authorized_User()
        {
            // Act
            var cut = RenderComponent<CandidateDelete>(parameters => parameters.Add(p => p.candidateId, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("POTWIERDŹ USUNIĘCIE KANDYDATA", cut.Markup);
                Assert.Contains("Jan", cut.Markup);
                Assert.Contains("Kowalski", cut.Markup);
                Assert.Contains("12345678901", cut.Markup);
                Assert.Contains("Inżynier", cut.Markup);
                Assert.Contains("Warszawa", cut.Markup);
            });
        }

        [Fact]
        public async Task CandidateDelete_Should_Handle_Invalid_CandidateId()
        {
            // Arrange
            var candidateId = 99;

            _candidateManagementServiceMock.Setup(s => s.CandidateExists(candidateId)).ReturnsAsync(false);

            var cut = RenderComponent<CandidateDelete>(parameters => parameters.Add(p => p.candidateId, candidateId));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nie znaleziono takiego kandydata!", cut.Markup);
            });
        }

        [Fact]
        public async Task CandidateDelete_Should_Delete_Candidate_On_Valid_Submission()
        {
            // Arrange
            var candidateId = 1;

            _candidateManagementServiceMock.Setup(s => s.CandidateExists(candidateId)).ReturnsAsync(true);
            _candidateManagementServiceMock.Setup(s => s.GetCandidateById(candidateId)).ReturnsAsync(new CandidateViewModel
            {
                IdCandidate = 1,
                IdPerson = 1,
                JobType = "Inżynier",
                PositionNumber = 2,
                PlaceOfResidence = "Warszawa"
            });
            _personManagementServiceMock.Setup(s => s.GetPersonById(1)).ReturnsAsync(new PersonViewModel
            {
                Name = "Jan",
                Surname = "Kowalski",
                PESEL = "51032539655",
                DateOfBirthString = "1951-03-25"
            });
            _candidateManagementServiceMock.Setup(s => s.DeleteCandidate(candidateId)).ReturnsAsync(true);

            var cut = RenderComponent<CandidateDelete>(parameters => parameters.Add(p => p.candidateId, candidateId));

            // Act
            var deleteButton = cut.Find("button.red-submit-button");
            await cut.InvokeAsync(() => deleteButton.Click());

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Kandydat usunięty pomyślnie!", cut.Markup);
            });
        }

        [Fact]
        public void CandidateDelete_Should_Render_NotAuthorized_For_Unauthorized_User()
        {
            // Arrange
            var unauthorizedAuthState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(unauthorizedAuthState));

            // Act
            var cut = RenderComponent<CandidateDelete>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nie posiadasz odpowiednich uprawnień do wyświetlenia tej strony", cut.Markup);
                Assert.Contains("WarningIcon.png", cut.Markup);
            });
        }
        [Fact]
        public async Task CandidateDelete_Should_Navigate_To_CandidateList_On_Cancel()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<CandidateDelete>(parameters => parameters.Add(p => p.candidateId, 1));
            var cancelButton = cut.Find("button.cancel-button");
            cancelButton.Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.EndsWith("/candidatelist", navigationManager.Uri);
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
