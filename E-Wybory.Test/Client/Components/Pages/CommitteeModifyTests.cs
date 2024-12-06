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
using E_Wybory.Client.Services;
using Microsoft.AspNetCore.Components;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class CommitteeModifyTests : TestContext
    {
        private readonly Mock<IPartyManagementService> _partyManagementServiceMock;

        public CommitteeModifyTests()
        {
            _partyManagementServiceMock = new Mock<IPartyManagementService>();
            _partyManagementServiceMock.Setup(service => service.PartyExists(It.IsAny<int>())).ReturnsAsync(true);
            _partyManagementServiceMock.Setup(service => service.GetPartyById(It.IsAny<int>())).ReturnsAsync(new PartyViewModel
            {
                IdParty = 1,
                ListCommiteeNumber = 1,
                PartyName = "Komitet Testowy",
                Abbreviation = "KT",
                PartyAddress = "Warszawa",
                PartyType = "Polityczna",
                IsCoalition = true,
                Website = "www.komitettestowy.pl"
            });
            _partyManagementServiceMock.Setup(service => service.PutParty(It.IsAny<PartyViewModel>())).ReturnsAsync(true);

            Services.AddSingleton(_partyManagementServiceMock.Object);

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
        public void CommitteeModify_Should_Render_Correctly_For_Authorized_User()
        {
            // Act
            var cut = RenderComponent<CommitteeModify>(parameters => parameters.Add(p => p.committeeId, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Console.WriteLine(cut.Markup);
                Assert.Contains("MODYFIKOWANIE KOMITETU WYBORCZEGO", cut.Markup);
                Assert.Contains("NR LISTY", cut.Markup);
                Assert.Contains("NAZWA KOMITETU", cut.Markup);
                Assert.Contains("SKRÓT", cut.Markup);
                Assert.Contains("ADRES", cut.Markup);
                Assert.Contains("TYP PARTII", cut.Markup);
                Assert.Contains("PARTIA KOALICYJNA", cut.Markup);
                Assert.Contains("STRONA WWW", cut.Markup);
                Assert.Contains("MODYFIKUJ", cut.Markup);
                Assert.Contains("ANULUJ", cut.Markup);
            });
        }

        [Fact]
        public void CommitteeModify_Should_Render_NotAuthorized_For_Unauthorized_User()
        {
            // Arrange
            var unauthorizedAuthState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(unauthorizedAuthState));

            // Act
            var cut = RenderComponent<CommitteeModify>(parameters => parameters.Add(p => p.committeeId, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nie posiadasz odpowiednich uprawnień do wyświetlenia tej strony", cut.Markup);
                Assert.Contains("WarningIcon.png", cut.Markup);
            });
        }

        [Fact]
        public void CommitteeModify_Should_Modify_Committee_On_ValidSubmission()
        {
            // Act
            var cut = RenderComponent<CommitteeModify>(parameters => parameters.Add(p => p.committeeId, 1));
            var submitButton = cut.Find("button.submit-button");
            submitButton.Click();

            // Assert
            _partyManagementServiceMock.Verify(service => service.PutParty(It.IsAny<PartyViewModel>()), Times.Once);
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Modyfikacja komitetu się udała!", cut.Markup);
            });
        }

        [Fact]
        public void CommitteeModify_Should_Navigate_To_CommitteeList_On_Cancel()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<CommitteeModify>(parameters => parameters.Add(p => p.committeeId, 1));
            var cancelButton = cut.Find("button.cancel-button");
            cancelButton.Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.EndsWith("/committeelist", navigationManager.Uri);
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
