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
    public class CandidateModifyTests : TestContext
    {
        private readonly Mock<IPersonManagementService> _personManagementServiceMock;
        private readonly Mock<ICandidateManagementService> _candidateManagementServiceMock;
        private readonly Mock<IDistrictManagementService> _districtManagementServiceMock;
        private readonly Mock<IPartyManagementService> _partyManagementServiceMock;
        private readonly Mock<IElectionManagementService> _electionManagementServiceMock;

        public CandidateModifyTests()
        {
            _personManagementServiceMock = new Mock<IPersonManagementService>();
            _candidateManagementServiceMock = new Mock<ICandidateManagementService>();
            _districtManagementServiceMock = new Mock<IDistrictManagementService>();
            _partyManagementServiceMock = new Mock<IPartyManagementService>();
            _electionManagementServiceMock = new Mock<IElectionManagementService>();

            Services.AddSingleton(_personManagementServiceMock.Object);
            Services.AddSingleton(_candidateManagementServiceMock.Object);
            Services.AddSingleton(_districtManagementServiceMock.Object);
            Services.AddSingleton(_partyManagementServiceMock.Object);
            Services.AddSingleton(_electionManagementServiceMock.Object);

            var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Administratorzy"),
            }, "test"))));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(authState));

            Services.AddAuthorizationCore();
            Services.AddSingleton<IAuthorizationPolicyProvider, DefaultAuthorizationPolicyProvider>();
            Services.AddSingleton<IAuthorizationService, DefaultAuthorizationService>();
        }

        [Fact]
        public async Task CandidateModify_Should_Render_Correctly()
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
            _personManagementServiceMock.Setup(s => s.GetPersonById(It.IsAny<int>())).ReturnsAsync(personViewModel);
            _districtManagementServiceMock.Setup(s => s.Districts()).ReturnsAsync(districts);
            _partyManagementServiceMock.Setup(s => s.Parties()).ReturnsAsync(parties);
            _electionManagementServiceMock.Setup(s => s.Elections()).ReturnsAsync(elections);

            // Act
            var cut = RenderComponent<CandidateModify>(parameters => parameters.Add(p => p.candidateId, candidateId));

            // Assert
            Assert.NotNull(cut.Find("form.candidate-form"));
            Assert.Contains("MODYFIKOWANIE KANDYDATA W WYBORACH", cut.Markup);

            // Validate inputs
            var nameInput = cut.Find("input#firstName");
            Assert.Equal("John", nameInput.GetAttribute("value"));

            var surnameInput = cut.Find("input#lastName");
            Assert.Equal("Doe", surnameInput.GetAttribute("value"));

            var peselInput = cut.Find("input#pesel");
            Assert.Equal("12345678901", peselInput.GetAttribute("value"));

            var birthDateInput = cut.Find("input#dateBirth");
            Assert.Equal("1990-01-01", birthDateInput.GetAttribute("value"));

            var jobTypeInput = cut.Find("input#occupation");
            Assert.Equal("Software Engineer", jobTypeInput.GetAttribute("value"));

            var workplaceInput = cut.Find("input#workplace");
            Assert.Equal("TechCorp", workplaceInput.GetAttribute("value"));

            var campaignDescriptionInput = cut.Find("input#campaign");
            Assert.Equal("For a better future", campaignDescriptionInput.GetAttribute("value"));

            var positionNumberInput = cut.Find("input#listNumber");
            Assert.Equal("1", positionNumberInput.GetAttribute("value"));

            // Validate selects
            var educationSelect = cut.Find("select#education");
            Assert.Equal("Wyższe", educationSelect.GetAttribute("value"));

            var partySelect = cut.Find("select#partyAffiliation");
            Assert.Equal("1", partySelect.GetAttribute("value"));

            var electionSelect = cut.Find("select#election");
            Assert.Equal("1", electionSelect.GetAttribute("value"));

            var districtSelect = cut.Find("select#districtNumber");
            Assert.Equal("1", districtSelect.GetAttribute("value"));

            // Validate buttons
            Assert.NotNull(cut.Find("button.submit-button"));
            Assert.NotNull(cut.Find("button.cancel-button"));
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
