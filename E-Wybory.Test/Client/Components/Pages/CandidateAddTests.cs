using Bunit;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using Moq;
using E_Wybory.Client.Components.Pages;
using E_Wybory.Client.Services;
using E_Wybory.Client.ViewModels;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class CandidateAddTests : TestContext
    {
        private readonly Mock<IPersonManagementService> _personManagementServiceMock;
        private readonly Mock<ICandidateManagementService> _candidateManagementServiceMock;
        private readonly Mock<IDistrictManagementService> _districtManagementServiceMock;
        private readonly Mock<IPartyManagementService> _partyManagementServiceMock;
        private readonly Mock<IElectionManagementService> _electionManagementServiceMock;

        public CandidateAddTests()
        {
            _personManagementServiceMock = new Mock<IPersonManagementService>();
            _candidateManagementServiceMock = new Mock<ICandidateManagementService>();
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


            _personManagementServiceMock.Setup(s => s.GetPersonIdByPeselAsync(It.IsAny<string>())).ReturnsAsync(0);
            _personManagementServiceMock.Setup(s => s.AddPerson(It.IsAny<PersonViewModel>())).ReturnsAsync(true);
            _candidateManagementServiceMock.Setup(s => s.AddCandidate(It.IsAny<CandidateViewModel>())).ReturnsAsync(true);


            Services.AddSingleton(_personManagementServiceMock.Object);
            Services.AddSingleton(_candidateManagementServiceMock.Object);
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
        public void CandidateAdd_Should_Render_Correctly_For_Authorized_User()
        {

            // Act
            var cut = RenderComponent<CandidateAdd>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Console.WriteLine(cut.Markup);
                Assert.Contains("DODAWANIE KANDYDATA W WYBORACH", cut.Markup);
                Assert.Contains("IMIONA", cut.Markup);
                Assert.Contains("NAZWISKO", cut.Markup);
                Assert.Contains("DATA URODZENIA", cut.Markup);
                Assert.Contains("PESEL", cut.Markup);
                Assert.Contains("ZAWÓD", cut.Markup);
                Assert.Contains("MIEJSCE PRACY", cut.Markup);
                Assert.Contains("NUMER NA LIŚCIE", cut.Markup);
                Assert.Contains("WYKSZTAŁCENIE", cut.Markup);
                Assert.Contains("PRZYNALEŻNOŚĆ DO PARTII", cut.Markup);
                Assert.Contains("MIEJSCE ZAMIESZKANIA", cut.Markup);
                Assert.Contains("OBWÓD WYBORCZY", cut.Markup);
            });
        }

        [Fact]
        public void CandidateAdd_Should_Render_NotAuthorized_For_Unauthorized_User()
        {
            // Arrange
            var unauthorizedAuthState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(unauthorizedAuthState));

            // Act
            var cut = RenderComponent<CandidateAdd>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nie posiadasz odpowiednich uprawnień do wyświetlenia tej strony", cut.Markup);
                Assert.Contains("WarningIcon.png", cut.Markup);
            });
        }

        [Fact]
        public async Task CandidateAdd_Should_Handle_Candidate_Added_Successfully()
        {
            // Arrange
            
            var cut = RenderComponent<CandidateAdd>();

            cut.Find("input#name").Change("Jan");
            cut.Find("input#lastName").Change("Kowalski");
            cut.Find("input#birthDate").Change("2002-07-02");
            cut.Find("input#pesel").Change("02270205457");
            cut.Find("input#occupation").Change("Inżynier");
            cut.Find("input#workplace").Change("Firma XYZ");
            cut.Find("input#campaign").Change("Rozwój infrastruktury");
            cut.Find("input#listNumber").Change("1");
            cut.Find("select#education").Change("Wyższe");
            cut.Find("select#partyAffiliation").Change("1");
            cut.Find("select#election").Change("1");
            cut.Find("select#districtNumber").Change("1");
            cut.Find("input#placeOfResidence").Change("Warszawa");

            // Act
            var submitButton = cut.Find("button.submit-button");
            await cut.InvokeAsync(() => submitButton.Click());

            // Assert
            cut.WaitForAssertion(() =>
            {
                Console.WriteLine(cut.Markup);
                Assert.Contains("Dodawanie kandydata sie powiodło!", cut.Markup);
            });
        }

        [Fact]
        public async Task CandidateAdd_Should_Handle_Invalid_Pesel()
        {
            // Arrange & Act
            var cut = RenderComponent<CandidateAdd>();
            cut.Find("input#pesel").Change("12345abc");

            // Assert
            cut.WaitForAssertion(() =>
            {
                Console.WriteLine(cut.Markup);
                Assert.Contains("PESEL musi składać się z dokładnie 11 cyfr.", cut.Markup);
                Assert.Contains("PESEL musi zawierać tylko cyfry.", cut.Markup);
            });
        }

        [Fact]
        public async Task CandidateAdd_Should_Navigate_To_CandidateList_On_Cancel()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<CandidateAdd>();
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

            public override Task<AuthenticationState> GetAuthenticationStateAsync()
            {
                return _authenticationState;
            }
        }
    }
}
