using Bunit;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using E_Wybory.Client.Components.Pages;
using E_Wybory.Client.Services;
using Moq;
using E_Wybory.Client.FilterData;
using E_Wybory.Client.ViewModels;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class CandidateListTests : TestContext
    {
        private readonly Mock<IFilterWrapperManagementService> _filterWrapperServiceMock;
        private readonly Mock<IPartyManagementService> _partyManagementServiceMock;
        private readonly Mock<IElectionTypeManagementService> _electionTypeManagementServiceMock;
        private readonly Mock<IPersonManagementService> _personManagementServiceMock;

        public CandidateListTests()
        {
            _filterWrapperServiceMock = new Mock<IFilterWrapperManagementService>();
            _partyManagementServiceMock = new Mock<IPartyManagementService>();
            _electionTypeManagementServiceMock = new Mock<IElectionTypeManagementService>();
            _personManagementServiceMock = new Mock<IPersonManagementService>();

            _filterWrapperServiceMock.Setup(service => service.GetFilteredCandidates(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<CandidatePersonViewModel>
                {
                    new CandidatePersonViewModel
                    {
                        personViewModel = new PersonViewModel
                        {
                            Name = "Jan",
                            Surname = "Kowalski",
                            BirthDate = new System.DateTime(1985, 5, 10)
                        },
                        candidateViewModel = new CandidateViewModel
                        {
                            PositionNumber = 1,
                            JobType = "Inżynier",
                            Workplace = "Firma A",
                            PlaceOfResidence = "Warszawa",
                            EducationStatus = "Wyższe",
                            IdParty = 1
                        }
                    },
                    new CandidatePersonViewModel
                    {
                        personViewModel = new PersonViewModel
                        {
                            Name = "Anna",
                            Surname = "Nowak",
                            BirthDate = new System.DateTime(1990, 8, 15)
                        },
                        candidateViewModel = new CandidateViewModel
                        {
                            PositionNumber = 2,
                            JobType = "Lekarz",
                            Workplace = "Szpital B",
                            PlaceOfResidence = "Kraków",
                            EducationStatus = "Wyższe",
                            IdParty = 2
                        }
                    }
                });

            _partyManagementServiceMock.Setup(service => service.Parties())
                .ReturnsAsync(new List<PartyViewModel>
                {
                    new PartyViewModel { IdParty = 1, PartyName = "Partia A" },
                    new PartyViewModel { IdParty = 2, PartyName = "Partia B" }
                });

            _partyManagementServiceMock.Setup(service => service.GetPartyNameById(It.Is<int>(id => id == 1), It.IsAny<List<PartyViewModel>>()))
                .Returns("Partia A");

            _partyManagementServiceMock.Setup(service => service.GetPartyNameById(It.Is<int>(id => id == 2), It.IsAny<List<PartyViewModel>>()))
                .Returns("Partia B");

            _electionTypeManagementServiceMock.Setup(service => service.ElectionTypes())
                .ReturnsAsync(new List<ElectionTypeViewModel>
                {
                    new ElectionTypeViewModel { IdElectionType = 1, ElectionTypeName = "Parlamentarne" },
                    new ElectionTypeViewModel { IdElectionType = 2, ElectionTypeName = "Samorządowe" }
                });

            _personManagementServiceMock.Setup(service => service.CountPersonAge(It.IsAny<System.DateTime>()))
                .Returns(36);

            Services.AddSingleton(_filterWrapperServiceMock.Object);
            Services.AddSingleton(_partyManagementServiceMock.Object);
            Services.AddSingleton(_electionTypeManagementServiceMock.Object);
            Services.AddSingleton(_personManagementServiceMock.Object);

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
        public void CandidateList_Should_Render_Correctly_For_Authorized_User()
        {
            // Act
            var cut = RenderComponent<CandidateList>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("KONFIGURACJA KANDYDATÓW", cut.Markup);
                Assert.Contains("DODAJ KANDYDATA", cut.Markup);
                Assert.Contains("Nazwisko i imiona", cut.Markup);
                Assert.Contains("Zawód", cut.Markup);
                Assert.Contains("Miejsce (zakład) pracy", cut.Markup);
                Assert.Contains("Miejscowość zamieszkania", cut.Markup);
                Assert.Contains("Wykształcenie", cut.Markup);
                Assert.Contains("Wiek", cut.Markup);
                Assert.Contains("Przynależność do partii politycznej", cut.Markup);
                Assert.Contains("Numer na liście", cut.Markup);
                Assert.Contains("Operacje konfiguracji", cut.Markup);
            });
        }
        [Fact]
        public async Task CandidateList_Should_Display_Link_To_Modify_And_Delete()
        {
            // Arrange
            var cut = RenderComponent<CandidateList>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Console.WriteLine(cut.Markup);
                Assert.Contains("/modifycandidate/0", cut.Markup);
                Assert.Contains("/deletecandidate/0", cut.Markup);
            });
        }
        [Fact]
        public async Task CandidateList_Should_Display_Filtered_Candidates()
        {
            // Act
            var cut = RenderComponent<CandidateList>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Kowalski Jan", cut.Markup);
                Assert.Contains("Inżynier", cut.Markup);
                Assert.Contains("Firma A", cut.Markup);
                Assert.Contains("Warszawa", cut.Markup);
                Assert.Contains("Wyższe", cut.Markup);
                Assert.Contains("Partia A", cut.Markup);

                Assert.Contains("Nowak Anna", cut.Markup);
                Assert.Contains("Lekarz", cut.Markup);
                Assert.Contains("Szpital B", cut.Markup);
                Assert.Contains("Kraków", cut.Markup);
                Assert.Contains("Partia B", cut.Markup);
            });
        }

        [Fact]
        public void CandidateList_Should_Render_NotAuthorized_For_Unauthorized_User()
        {
            // Arrange
            var unauthorizedAuthState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(unauthorizedAuthState));

            // Act
            var cut = RenderComponent<CandidateList>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nie posiadasz odpowiednich uprawnień do wyświetlenia tej strony", cut.Markup);
                Assert.Contains("WarningIcon.png", cut.Markup);
            });
        }
        [Fact]
        public void CandidateList_Should_Navigate_To_AddCandidate_On_AddButton_Click()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<CandidateList>();
            var addButton = cut.Find("button.add-button");
            addButton.Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.EndsWith("/addcandidate", navigationManager.Uri);
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
