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
using E_Wybory.Client.FilterData;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class DistrictModifyTests : TestContext
    {
        private readonly Mock<IDistrictManagementService> _districtManagementServiceMock;
        private readonly Mock<IConstituencyManagementService> _constituencyManagementServiceMock;
        private readonly Mock<IVoivodeshipManagementService> _voivodeshipManagementServiceMock;
        private readonly Mock<IFilterWrapperManagementService> _filterWrapperServiceMock;

        public DistrictModifyTests()
        {
            _districtManagementServiceMock = new Mock<IDistrictManagementService>();
            _districtManagementServiceMock
                .Setup(service => service.DistrictExists(It.IsAny<int>()))
                .ReturnsAsync(true);

            _districtManagementServiceMock
                .Setup(service => service.GetDistrictById(It.IsAny<int>()))
                .ReturnsAsync(new DistrictViewModel
                {
                    IdDistrict = 1,
                    DistrictName = "Obwód 1",
                    DistrictHeadquarters = "Siedziba 1",
                    DisabledFacilities = true,
                    IdConstituency = 1
                });

            _districtManagementServiceMock
                .Setup(service => service.PutDistrict(It.IsAny<DistrictViewModel>()))
                .ReturnsAsync(true);

            Services.AddSingleton(_districtManagementServiceMock.Object);

            _constituencyManagementServiceMock = new Mock<IConstituencyManagementService>();
            _constituencyManagementServiceMock
                .Setup(service => service.Constituences())
                .ReturnsAsync(new List<ConstituencyViewModel>
                {
                    new ConstituencyViewModel { idConstituency = 1, constituencyName = "Okręg 1" }
                });
            Services.AddSingleton(_constituencyManagementServiceMock.Object);

            _voivodeshipManagementServiceMock = new Mock<IVoivodeshipManagementService>();
            _voivodeshipManagementServiceMock
                .Setup(service => service.Voivodeships())
                .ReturnsAsync(new List<VoivodeshipViewModel>
                {
                    new VoivodeshipViewModel { idVoivodeship = 1, voivodeshipName = "Mazowieckie" }
                });
            Services.AddSingleton(_voivodeshipManagementServiceMock.Object);

            _filterWrapperServiceMock = new Mock<IFilterWrapperManagementService>();
            _filterWrapperServiceMock
                .Setup(service => service.GetFilteredLists(It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(new FilterListWrapper
                {
                    VoivodeshipFilter = new List<VoivodeshipViewModel>
                    {
                        new VoivodeshipViewModel { idVoivodeship = 1, voivodeshipName = "Mazowieckie" }
                    },
                    CountyFilter = new List<CountyViewModel>
                    {
                        new CountyViewModel { IdCounty = 1, CountyName = "Powiat 1", IdVoivodeship = 1 }
                    },
                    ProvinceFilter = new List<ProvinceViewModel>
                    {
                        new ProvinceViewModel { IdProvince = 1, ProvinceName = "Gmina 1", IdCounty = 1 }
                    }
                });
            Services.AddSingleton(_filterWrapperServiceMock.Object);

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
        public void DistrictModify_Should_Render_Correctly_For_Authorized_User()
        {
            // Act
            var cut = RenderComponent<DistrictModify>(parameters => parameters.Add(p => p.districtId, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("MODYFIKOWANIE OBWODU WYBORCZEGO", cut.Markup);
                Assert.Contains("Obwód 1", cut.Markup);
                Assert.Contains("ZATWIERDŹ ZMIANY", cut.Markup);
                Assert.Contains("ANULUJ", cut.Markup);
            });
        }

        [Fact]
        public void DistrictModify_Should_Render_NotAuthorized_For_Unauthorized_User()
        {
            // Arrange
            var unauthorizedAuthState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(unauthorizedAuthState));

            // Act
            var cut = RenderComponent<DistrictModify>(parameters => parameters.Add(p => p.districtId, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nie posiadasz odpowiednich uprawnień do wyświetlenia tej strony", cut.Markup);
                Assert.Contains("WarningIcon.png", cut.Markup);
            });
        }

        [Fact]
        public void DistrictModify_Should_Navigate_To_DistrictList_On_Cancel()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<DistrictModify>(parameters => parameters.Add(p => p.districtId, 1));
            var cancelButton = cut.Find("button.cancel-button");
            cancelButton.Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.EndsWith("/districtlist", navigationManager.Uri);
            });
        }

        [Fact]
        public void DistrictModify_Should_Call_Service_On_ValidSubmission()
        {
            // Arrange
            var cut = RenderComponent<DistrictModify>(parameters => parameters.Add(p => p.districtId, 1));
            var form = cut.Find("form");

            // Act
            form.Submit();

            // Assert
            _districtManagementServiceMock.Verify(service => service.PutDistrict(It.IsAny<DistrictViewModel>()), Times.Once);
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
