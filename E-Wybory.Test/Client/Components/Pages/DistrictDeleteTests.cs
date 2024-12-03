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
    public class DistrictDeleteTests : TestContext
    {
        private readonly Mock<IDistrictManagementService> _districtServiceMock;
        private readonly Mock<IProvinceManagementService> _provinceServiceMock;
        private readonly Mock<ICountyManagementService> _countyServiceMock;
        private readonly Mock<IVoivodeshipManagementService> _voivodeshipServiceMock;

        public DistrictDeleteTests()
        {
            _districtServiceMock = new Mock<IDistrictManagementService>();
            _provinceServiceMock = new Mock<IProvinceManagementService>();
            _countyServiceMock = new Mock<ICountyManagementService>();
            _voivodeshipServiceMock = new Mock<IVoivodeshipManagementService>();

            _districtServiceMock
                .Setup(service => service.DistrictExists(It.IsAny<int>()))
                .ReturnsAsync(true);

            _districtServiceMock
                .Setup(service => service.GetDistrictById(It.IsAny<int>()))
                .ReturnsAsync(new DistrictViewModel
                {
                    DistrictName = "Test District",
                    DistrictHeadquarters = "Test Headquarters",
                    IdProvince = 1
                });

            _provinceServiceMock
                .Setup(service => service.GetProvinceById(It.IsAny<int>()))
                .ReturnsAsync(new ProvinceViewModel
                {
                    ProvinceName = "Test Province",
                    IdCounty = 1
                });

            _countyServiceMock
                .Setup(service => service.GetCountyById(It.IsAny<int>()))
                .ReturnsAsync(new CountyViewModel
                {
                    CountyName = "Test County",
                    IdVoivodeship = 1
                });

            _voivodeshipServiceMock
                .Setup(service => service.GetVoivodeshipById(It.IsAny<int>()))
                .ReturnsAsync(new VoivodeshipViewModel
                {
                    voivodeshipName = "Test Voivodeship"
                });

            _districtServiceMock
                .Setup(service => service.DeleteDistrict(It.IsAny<int>()))
                .ReturnsAsync(true);

            Services.AddSingleton(_districtServiceMock.Object);
            Services.AddSingleton(_provinceServiceMock.Object);
            Services.AddSingleton(_countyServiceMock.Object);
            Services.AddSingleton(_voivodeshipServiceMock.Object);

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
        public void DistrictDelete_Should_Render_Correctly_For_Authorized_User()
        {
            // Act
            var cut = RenderComponent<DistrictDelete>(parameters => parameters.Add(p => p.districtId, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("USUWANIE OBWODU WYBORCZEGO", cut.Markup);
                Assert.Contains("Test District", cut.Markup);
                Assert.Contains("Test Headquarters", cut.Markup);
                Assert.Contains("Test Province", cut.Markup);
                Assert.Contains("Test County", cut.Markup);
                Assert.Contains("Test Voivodeship", cut.Markup);
            });
        }

        [Fact]
        public void DistrictDelete_Should_Render_NotAuthorized_For_Unauthorized_User()
        {
            // Arrange
            var unauthorizedAuthState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(unauthorizedAuthState));

            // Act
            var cut = RenderComponent<DistrictDelete>(parameters => parameters.Add(p => p.districtId, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nie posiadasz odpowiednich uprawnień do wyświetlenia tej strony", cut.Markup);
                Assert.Contains("WarningIcon.png", cut.Markup);
            });
        }

        [Fact]
        public void DistrictDelete_Should_Navigate_To_DistrictList_On_Cancel()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<DistrictDelete>(parameters => parameters.Add(p => p.districtId, 1));
            var cancelButton = cut.Find("button.submit-button");
            cancelButton.Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.EndsWith("/districtlist", navigationManager.Uri);
            });
        }

        [Fact]
        public async Task DistrictDelete_Should_Call_Service_On_ValidSubmission()
        {
            // Arrange
            var cut = RenderComponent<DistrictDelete>(parameters => parameters.Add(p => p.districtId, 1));
            var form = cut.Find("form");

            // Act
            form.Submit();

            // Assert
            await Task.Delay(500);
            _districtServiceMock.Verify(service => service.DeleteDistrict(It.Is<int>(id => id == 1)), Times.Once);
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
