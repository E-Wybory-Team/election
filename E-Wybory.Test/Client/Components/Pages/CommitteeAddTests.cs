using Bunit;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using E_Wybory.Client.Components.Pages;
using E_Wybory.Client.Services;
using E_Wybory.Client.ViewModels;
using Microsoft.AspNetCore.Components;
using Moq;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class CommitteeAddTests : TestContext
    {
        public CommitteeAddTests()
        {
            var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Role, "Pracownicy PKW"),
            }, "test"))));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(authState));

            Services.AddAuthorizationCore();
            Services.AddSingleton<IAuthorizationPolicyProvider, DefaultAuthorizationPolicyProvider>();
            Services.AddSingleton<IAuthorizationService, DefaultAuthorizationService>();

            var partyServiceMock = new Mock<IPartyManagementService>();
            partyServiceMock.Setup(service => service.AddParty(It.IsAny<PartyViewModel>())).ReturnsAsync(true);
            Services.AddSingleton(partyServiceMock.Object);
        }

        [Fact]
        public void CommitteeAdd_Should_Render_Correctly_For_Authorized_User()
        {
            // Act
            var cut = RenderComponent<CommitteeAdd>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("DODAWANIE KOMITETU WYBORCZEGO", cut.Markup);
                Assert.Contains("NR LISTY", cut.Markup);
                Assert.Contains("NAZWA KOMITETU", cut.Markup);
                Assert.Contains("PARTIA KOALICYJNA", cut.Markup);
                Assert.Contains("DODAJ", cut.Markup);
                Assert.Contains("ANULUJ", cut.Markup);
            });
        }

        [Fact]
        public void CommitteeAdd_Should_Render_NotAuthorized_For_Unauthorized_User()
        {
            // Arrange
            var unauthorizedAuthState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(unauthorizedAuthState));

            // Act
            var cut = RenderComponent<CommitteeAdd>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nie posiadasz odpowiednich uprawnień do wyświetlenia tej strony", cut.Markup);
                Assert.Contains("WarningIcon.png", cut.Markup);
            });
        }

        [Fact]
        public void CommitteeAdd_Should_Call_HandleAddSubmit_On_Submit()
        {
            // Arrange
            var cut = RenderComponent<CommitteeAdd>();

            cut.Find("input#nr-listy").Change("1");
            cut.Find("input#nazwa").Change("Nowa Partia");
            cut.Find("input#skrot").Change("NP");
            cut.Find("input#adres").Change("Warszawa");
            cut.Find("input#type").Change("Polityczna");
            cut.Find("input#website").Change("www.nowapartia.pl");

            // Act
            var submitButton = cut.Find("button.submit-button");
            submitButton.Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Adding committee successful!", cut.Markup);
            });
        }

        [Fact]
        public void CommitteeAdd_Should_Navigate_To_CommitteeList_On_Cancel()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<CommitteeAdd>();
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
