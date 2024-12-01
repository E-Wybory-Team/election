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

            Services.AddSingleton(_personManagementServiceMock.Object);
            Services.AddSingleton(_candidateManagementServiceMock.Object);
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
        public void CandidateAdd_Should_Render_Correctly()
        {
            // Arrange
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

            _districtManagementServiceMock.Setup(s => s.Districts()).ReturnsAsync(districts);
            _partyManagementServiceMock.Setup(s => s.Parties()).ReturnsAsync(parties);
            _electionManagementServiceMock.Setup(s => s.Elections()).ReturnsAsync(elections);

            // Act
            var cut = RenderComponent<CandidateAdd>();

            // Assert
            var form = cut.Find("form.candidate-form");
            Assert.NotNull(form);

            var nameInput = cut.Find("input#name");
            Assert.Equal("Wprowadź imiona kandydata", nameInput.GetAttribute("placeholder"));

            var lastNameInput = cut.Find("input#lastName");
            Assert.Equal("Wprowadź nazwisko kandydata", lastNameInput.GetAttribute("placeholder"));

            var peselInput = cut.Find("input#pesel");
            Assert.Equal("Wprowadź nr PESEL kandydata", peselInput.GetAttribute("placeholder"));

            var listNumberInput = cut.Find("input#listNumber");
            Assert.Equal("number", listNumberInput.GetAttribute("type"));

            var districtSelect = cut.Find("select#districtNumber");
            Assert.Contains("District 1 - HQ 1", districtSelect.InnerHtml);

            var partySelect = cut.Find("select#partyAffiliation");
            Assert.Contains("Party 1", partySelect.InnerHtml);

            var electionSelect = cut.Find("select#election");
            Assert.Contains("Wybierz wybory", electionSelect.InnerHtml);

            var submitButton = cut.Find("button.submit-button");
            Assert.Contains("DODAJ", submitButton.InnerHtml);

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
