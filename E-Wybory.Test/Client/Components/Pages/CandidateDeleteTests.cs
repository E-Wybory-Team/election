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
using System.Collections.Generic;

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

            Services.AddSingleton(_candidateManagementServiceMock.Object);
            Services.AddSingleton(_personManagementServiceMock.Object);
            Services.AddSingleton(_districtManagementServiceMock.Object);
            Services.AddSingleton(_partyManagementServiceMock.Object);
            Services.AddSingleton(_electionManagementServiceMock.Object);

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
        public void CandidateDelete_Should_Render_Correctly()
        {
            // Arrange
            var candidateId = 1;

            var candidateViewModel = new CandidateViewModel
            {
                IdCandidate = candidateId,
                JobType = "Software Engineer",
                Workplace = "TechCorp",
                CampaignDescription = "For a better future",
                PositionNumber = 1,
                EducationStatus = "Wyższe",
                IdParty = 1,
                IdElection = 1,
                IdDistrict = 1,
                PlaceOfResidence = "Cityville"
            };

            var personViewModel = new PersonViewModel
            {
                Name = "John",
                Surname = "Doe",
                PESEL = "12345678901",
                DateOfBirthString = "1990-01-01"
            };

            var districts = new List<DistrictViewModel>
            {
                new DistrictViewModel { IdDistrict = 1, DistrictName = "District 1", DistrictHeadquarters = "HQ 1" }
            };

            var parties = new List<PartyViewModel>
            {
                new PartyViewModel { IdParty = 1, PartyName = "Party 1" }
            };

            var elections = new List<ElectionViewModel>
            {
                new ElectionViewModel { IdElection = 1, ElectionStartDate = System.DateTime.Now, ElectionEndDate = System.DateTime.Now.AddDays(1) }
            };

            _candidateManagementServiceMock.Setup(s => s.CandidateExists(candidateId)).ReturnsAsync(true);
            _candidateManagementServiceMock.Setup(s => s.GetCandidateById(candidateId)).ReturnsAsync(candidateViewModel);
            _personManagementServiceMock.Setup(s => s.GetPersonById(candidateViewModel.IdPerson)).ReturnsAsync(personViewModel);
            _districtManagementServiceMock.Setup(s => s.Districts()).ReturnsAsync(districts);
            _partyManagementServiceMock.Setup(s => s.Parties()).ReturnsAsync(parties);
            _electionManagementServiceMock.Setup(s => s.Elections()).ReturnsAsync(elections);

            // Act
            var cut = RenderComponent<CandidateDelete>(parameters => parameters.Add(p => p.candidateId, candidateId));

            // Assert
            Assert.NotNull(cut.Find("form.candidate-form"));
            Assert.Contains("POTWIERDŹ USUNIĘCIE KANDYDATA", cut.Markup);

            // Validate readonly inputs
            Assert.Equal("John", cut.Find("input#firstName").GetAttribute("value"));
            Assert.Equal("Doe", cut.Find("input#lastName").GetAttribute("value"));
            Assert.Equal("12345678901", cut.Find("input#pesel").GetAttribute("value"));
            Assert.Equal("1990-01-01", cut.Find("input#dateBirth").GetAttribute("value"));
            Assert.Equal("Software Engineer", cut.Find("input#occupation").GetAttribute("value"));
            Assert.Equal("TechCorp", cut.Find("input#workplace").GetAttribute("value"));
            Assert.Equal("For a better future", cut.Find("input#campaign").GetAttribute("value"));
            Assert.Equal("1", cut.Find("input#listNumber").GetAttribute("value"));

            // Validate select dropdowns (disabled)
            Assert.Equal("Wyższe", cut.Find("select#education").GetAttribute("value"));
            Assert.Equal("1", cut.Find("select#partyAffiliation").GetAttribute("value"));
            Assert.Equal("1", cut.Find("select#election").GetAttribute("value"));
            Assert.Equal("1", cut.Find("select#districtNumber").GetAttribute("value"));

            // Validate buttons
            var deleteButton = cut.Find("button.red-submit-button");
            Assert.Contains("USUŃ", deleteButton.InnerHtml);

            var cancelButton = cut.Find("button.cancel-button");
            Assert.Contains("ANULUJ", cancelButton.InnerHtml);
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
