using Bunit;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Moq;
using E_Wybory.Client.Components.Pages;
using E_Wybory.Client.ViewModels;
using System.Collections.Generic;
using E_Wybory.Client.Services;
using Microsoft.AspNetCore.Components;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class CommitteeListTests : TestContext
    {
        private readonly Mock<IPartyManagementService> _partyManagementServiceMock;
        private readonly Mock<IElectionTypeManagementService> _electionTypeManagementServiceMock;

        public CommitteeListTests()
        {
            _partyManagementServiceMock = new Mock<IPartyManagementService>();
            _partyManagementServiceMock.Setup(service => service.Parties()).ReturnsAsync(new List<PartyViewModel>
            {
                new PartyViewModel
                {
                    IdParty = 1,
                    ListCommiteeNumber = 1,
                    PartyName = "Komitet Testowy",
                    Abbreviation = "KT",
                    PartyAddress = "Warszawa",
                    PartyType = "Polityczna",
                    IsCoalition = true,
                    Website = "www.komitettestowy.pl"
                },
                new PartyViewModel
                {
                    IdParty = 2,
                    ListCommiteeNumber = 2,
                    PartyName = "Komitet Przykładowy",
                    Abbreviation = "KP",
                    PartyAddress = "Kraków",
                    PartyType = "Społeczna",
                    IsCoalition = false,
                    Website = "www.komitetprzykladowy.pl"
                }
            });
            _partyManagementServiceMock.Setup(service => service.GetFilteredParties(It.IsAny<int?>())).ReturnsAsync(new List<PartyViewModel>
            {
                new PartyViewModel
                {
                    IdParty = 1,
                    ListCommiteeNumber = 1,
                    PartyName = "Komitet Testowy",
                    Abbreviation = "KT",
                    PartyAddress = "Warszawa",
                    PartyType = "Polityczna",
                    IsCoalition = true,
                    Website = "www.komitettestowy.pl"
                }
            });
            Services.AddSingleton(_partyManagementServiceMock.Object);

            _electionTypeManagementServiceMock = new Mock<IElectionTypeManagementService>();
            _electionTypeManagementServiceMock.Setup(service => service.ElectionTypes()).ReturnsAsync(new List<ElectionTypeViewModel>
            {
                new ElectionTypeViewModel { IdElectionType = 1, ElectionTypeName = "Parlamentarne" },
                new ElectionTypeViewModel { IdElectionType = 2, ElectionTypeName = "Prezydenckie" }
            });
            Services.AddSingleton(_electionTypeManagementServiceMock.Object);

            var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Role, "Pracownicy PKW"),
            }, "test"))));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(authState));

            Services.AddAuthorizationCore();
            Services.AddSingleton<IAuthorizationPolicyProvider, DefaultAuthorizationPolicyProvider>();
            Services.AddSingleton<IAuthorizationService, DefaultAuthorizationService>();
        }

        [Fact]
        public void CommitteeList_Should_Render_Correctly_For_Authorized_User()
        {
            // Act
            var cut = RenderComponent<CommitteeList>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("KONFIGURACJA KOMITETÓW WYBORCZYCH", cut.Markup);
                Assert.Contains("Komitet Testowy", cut.Markup);
                Assert.Contains("Komitet Przykładowy", cut.Markup);
                Assert.Contains("Dodaj komitet", cut.Markup);
                Assert.Contains("Nazwa", cut.Markup);
                Assert.Contains("Skrót", cut.Markup);
                Assert.Contains("Adres", cut.Markup);
                Assert.Contains("Rodzaj partii", cut.Markup);
                Assert.Contains("Partia koalicyjna", cut.Markup);
                Assert.Contains("Strona WWW", cut.Markup);
                Assert.Contains("Opcje konfiguracji", cut.Markup);
            });
        }

        [Fact]
        public void CommitteeList_Should_Render_NotAuthorized_For_Unauthorized_User()
        {
            // Arrange
            var unauthorizedAuthState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(unauthorizedAuthState));

            // Act
            var cut = RenderComponent<CommitteeList>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nie posiadasz odpowiednich uprawnień do wyświetlenia tej strony", cut.Markup);
                Assert.Contains("WarningIcon.png", cut.Markup);
            });
        }

        [Fact]
        public void CommitteeList_Should_Filter_Parties_By_ElectionType()
        {
            // Act
            var cut = RenderComponent<CommitteeList>();
            var filterDropdown = cut.Find("select");

            filterDropdown.Change("1");

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Komitet Testowy", cut.Markup);
                Assert.DoesNotContain("Komitet Przykładowy", cut.Markup);
            });
        }

        [Fact]
        public void CommitteeList_Should_Navigate_To_AddCommittee_On_AddButtonClick()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<CommitteeList>();
            var addButton = cut.Find("button.add-button");
            addButton.Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.EndsWith("/addcommittee", navigationManager.Uri);
            });
        }
        [Fact]
        public async Task CommitteeList_Should_Display_Link_To_Modify_And_Delete()
        {
            // Arrange
            var cut = RenderComponent<CommitteeList>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Console.WriteLine(cut.Markup);
                Assert.Contains("/modifycommittee/1", cut.Markup);
                Assert.Contains("/deletecommittee/1", cut.Markup);
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
