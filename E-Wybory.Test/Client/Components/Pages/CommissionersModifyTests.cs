using Bunit;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using E_Wybory.Client.Components.Pages;
using Microsoft.AspNetCore.Components;
using E_Wybory.Client.Services;
using Moq;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class CommissionersModifyTests : TestContext
    {

        private readonly Mock<IElectionUserManagementService> _electionUserManagementServiceMock;
        private readonly Mock<IPersonManagementService> _personManagementServiceMock;
        private readonly Mock<IFilterWrapperManagementService> _filterWrapperServiceMock;
        private readonly Mock<IUserTypeSetsManagementService> _userTypeSetsManagementServiceMock;
        private readonly Mock<IUserTypeManagementService> _userTypeManagementServiceMock;
        private readonly Mock<IVoivodeshipManagementService> _voivodeshipManagementServiceMock;
        public CommissionersModifyTests()
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
        public void CommissionersModify_Should_Render_Correctly_For_Authorized_User()
        {
            // Act
            var cut = RenderComponent<CommissionersModify>(parameters => parameters.Add(p => p.commissionerId, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("MODYFIKOWANIE CZŁONKA KOMISJI", cut.Markup);
                Assert.Contains("PESEL CZŁONKA KOMISJI", cut.Markup);
                Assert.Contains("85293128490", cut.Markup); // PESEL komisarza o ID 1
                Assert.Contains("Mazowieckie", cut.Markup);
                Assert.Contains("Powiat 1", cut.Markup);
                Assert.Contains("Gmina 1", cut.Markup);
                Assert.Contains("Obwód 1", cut.Markup);
                Assert.Contains("Przewodniczący", cut.Markup);
            });
        }

        [Fact]
        public void CommissionersModify_Should_Render_NotAuthorized_For_Unauthorized_User()
        {
            // Arrange
            var unauthorizedAuthState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(unauthorizedAuthState));

            // Act
            var cut = RenderComponent<CommissionersModify>(parameters => parameters.Add(p => p.commissionerId, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nie posiadasz odpowiednich uprawnień do wyświetlenia tej strony", cut.Markup);
                Assert.Contains("WarningIcon.png", cut.Markup);
            });
        }

        [Fact]
        public void CommissionersModify_Should_Navigate_To_CommissionersList_On_Cancel()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<CommissionersModify>(parameters => parameters.Add(p => p.commissionerId, 1));
            var cancelButton = cut.Find("button.cancel-button");
            cancelButton.Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.EndsWith("/commissionerslist", navigationManager.Uri);
            });
        }

        [Fact]
        public void CommissionersModify_Should_Navigate_To_CommissionersList_On_Submit()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<CommissionersModify>(parameters => parameters.Add(p => p.commissionerId, 1));
            var submitButton = cut.Find("button.submit-button");
            submitButton.Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.EndsWith("/commissionerslist", navigationManager.Uri);
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
