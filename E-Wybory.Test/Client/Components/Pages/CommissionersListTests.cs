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

namespace E_Wybory.Test.Client.Components.Pages
{
    public class CommissionersListTests : TestContext
    {
        private readonly Mock<IElectionUserManagementService> _electionUserManagementServiceMock;
        private readonly Mock<IPersonManagementService> _personManagementServiceMock;
        private readonly Mock<IFilterWrapperManagementService> _filterWrapperServiceMock;
        private readonly Mock<IUserTypeSetsManagementService> _userTypeSetsManagementServiceMock;
        private readonly Mock<IUserTypeManagementService> _userTypeManagementServiceMock;
        private readonly Mock<IVoivodeshipManagementService> _voivodeshipManagementServiceMock;

        public CommissionersListTests()
        {

            _electionUserManagementServiceMock = new Mock<IElectionUserManagementService>();
            _personManagementServiceMock = new Mock<IPersonManagementService>();
            _filterWrapperServiceMock = new Mock<IFilterWrapperManagementService>();
            _userTypeSetsManagementServiceMock = new Mock<IUserTypeSetsManagementService>();
            _userTypeManagementServiceMock = new Mock<IUserTypeManagementService>();
            _voivodeshipManagementServiceMock = new Mock<IVoivodeshipManagementService>();


            Services.AddSingleton(_electionUserManagementServiceMock.Object);
            Services.AddSingleton(_personManagementServiceMock.Object);
            Services.AddSingleton(_filterWrapperServiceMock.Object);
            Services.AddSingleton(_userTypeSetsManagementServiceMock.Object);
            Services.AddSingleton(_userTypeManagementServiceMock.Object);
            Services.AddSingleton(_voivodeshipManagementServiceMock.Object);

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
        public void CommissionersList_Should_Render_Correctly_For_Authorized_User()
        {
            // Act
            var cut = RenderComponent<CommissionersList>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("SPIS KOMISJI", cut.Markup);
                Assert.Contains("Dodaj członka", cut.Markup);
                Assert.Contains("Imię", cut.Markup);
                Assert.Contains("Nazwisko", cut.Markup);
                Assert.Contains("PESEL", cut.Markup);
                Assert.Contains("Telefon", cut.Markup);
                Assert.Contains("Adres email", cut.Markup);
                Assert.Contains("Stopień", cut.Markup);
                Assert.Contains("Opcje konfiguracji", cut.Markup);
            });
        }

        [Fact]
        public void CommissionersList_Should_Render_NotAuthorized_For_Unauthorized_User()
        {
            // Arrange
            var unauthorizedAuthState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(unauthorizedAuthState));

            // Act
            var cut = RenderComponent<CommissionersList>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nie posiadasz odpowiednich uprawnień do wyświetlenia tej strony", cut.Markup);
                Assert.Contains("WarningIcon.png", cut.Markup);
            });
        }

        [Fact]
        public void CommissionersList_Should_Display_Filtered_Commissioners()
        {
            // Act
            var cut = RenderComponent<CommissionersList>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Jan", cut.Markup);
                Assert.Contains("Kowalski", cut.Markup);
                Assert.Contains("jan.komisarz@gmail.com", cut.Markup);
                Assert.Contains("+48 9827319823", cut.Markup);
                Assert.Contains("Przewodniczący", cut.Markup);
                Assert.Contains("Modyfikuj", cut.Markup);
                Assert.Contains("Usuń", cut.Markup);
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
