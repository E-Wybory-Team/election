using Bunit;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Moq;
using E_Wybory.Client.ViewModels;
using E_Wybory.Client.Components.Pages;
using E_Wybory.Client.Services;
using Microsoft.AspNetCore.Components;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class DistrictAddTests : TestContext
    {
        private readonly Mock<IConstituencyManagementService> _constituencyServiceMock;
        private readonly Mock<IDistrictManagementService> _districtServiceMock;
        private readonly Mock<IVoivodeshipManagementService> _voivodeshipServiceMock;
        private readonly Mock<IFilterWrapperManagementService> _filterServiceMock;

        public DistrictAddTests()
        {
            _constituencyServiceMock = new Mock<IConstituencyManagementService>();
            _districtServiceMock = new Mock<IDistrictManagementService>();
            _voivodeshipServiceMock = new Mock<IVoivodeshipManagementService>();
            _filterServiceMock = new Mock<IFilterWrapperManagementService>();

            _constituencyServiceMock
                .Setup(service => service.Constituences())
                .ReturnsAsync(new List<ConstituencyViewModel>
                {
                    new ConstituencyViewModel { idConstituency = 1, constituencyName = "Warszawa" }
                });

            _voivodeshipServiceMock
                .Setup(service => service.Voivodeships())
                .ReturnsAsync(new List<VoivodeshipViewModel>
                {
                    new VoivodeshipViewModel { idVoivodeship = 1, voivodeshipName = "Mazowieckie" }
                });

            _districtServiceMock
                .Setup(service => service.AddDistrict(It.IsAny<DistrictViewModel>()))
                .ReturnsAsync(true);

            Services.AddSingleton(_constituencyServiceMock.Object);
            Services.AddSingleton(_districtServiceMock.Object);
            Services.AddSingleton(_voivodeshipServiceMock.Object);
            Services.AddSingleton(_filterServiceMock.Object);

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
        public void DistrictAdd_Should_Render_Correctly_For_Authorized_User()
        {
            // Act
            var cut = RenderComponent<DistrictAdd>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("DODAWANIE OBWODU WYBORCZEGO", cut.Markup);
                Assert.Contains("NAZWA OBWODU", cut.Markup);
                Assert.Contains("WOJEWÓDZTWO", cut.Markup);
                Assert.Contains("POWIAT", cut.Markup);
                Assert.Contains("OKRĘG", cut.Markup);
            });
        }

        [Fact]
        public void DistrictAdd_Should_Render_NotAuthorized_For_Unauthorized_User()
        {
            // Arrange
            var unauthorizedAuthState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(unauthorizedAuthState));

            // Act
            var cut = RenderComponent<DistrictAdd>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nie posiadasz odpowiednich uprawnień do wyświetlenia tej strony", cut.Markup);
                Assert.Contains("WarningIcon.png", cut.Markup);
            });
        }

        [Fact]
        public void DistrictAdd_Should_Navigate_To_DistrictList_On_Cancel()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<DistrictAdd>();
            var cancelButton = cut.Find("button.cancel-button");
            cancelButton.Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.EndsWith("/districtlist", navigationManager.Uri);
            });
        }

        [Fact]
        public async Task DistrictAdd_Should_Call_Service_On_ValidSubmission()
        {
            // Arrange
            var cut = RenderComponent<DistrictAdd>();
            var districtNameInput = cut.Find("#districtName");
            var districtHeadInput = cut.Find("#districtHead");

            // Act
            districtNameInput.Change("Test District");
            districtHeadInput.Change("Test Headquarters");

            var submitButton = cut.Find("button.submit-button");
            submitButton.Click();

            // Assert
            await Task.Delay(500);
            _districtServiceMock.Verify(service => service.AddDistrict(It.Is<DistrictViewModel>(
                d => d.DistrictName == "Test District" && d.DistrictHeadquarters == "Test Headquarters"
            )), Times.Once);
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
