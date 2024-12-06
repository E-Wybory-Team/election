using Bunit;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Moq;
using E_Wybory.Client.FilterData;
using E_Wybory.Client.ViewModels;
using E_Wybory.Client.Components.Pages;
using E_Wybory.Client.Services;
using Microsoft.AspNetCore.Components;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class DistrictListTests : TestContext
    {
        private readonly Mock<IFilterWrapperManagementService> _filterWrapperServiceMock;
        private readonly Mock<IConstituencyManagementService> _constituencyManagementServiceMock;
        private readonly Mock<IDistrictManagementService> _districtManagementServiceMock;

        public DistrictListTests()
        {
            _filterWrapperServiceMock = new Mock<IFilterWrapperManagementService>();
            _filterWrapperServiceMock
                .Setup(service => service.GetFilteredDistricts(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<DistrictViewModel>
                {
                    new DistrictViewModel { IdDistrict = 1, DistrictName = "Obwód 1", DistrictHeadquarters = "Siedziba 1", DisabledFacilities = true },
                    new DistrictViewModel { IdDistrict = 2, DistrictName = "Obwód 2", DistrictHeadquarters = "Siedziba 2", DisabledFacilities = false }
                });

            _filterWrapperServiceMock
                .Setup(service => service.GetFilteredListsDistricts(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(new FilterListWrapperDistrict
                {
                    ConstituencyFilter = new List<ConstituencyViewModel>
                    {
                        new ConstituencyViewModel { idConstituency = 1, constituencyName = "Okręg 1" },
                        new ConstituencyViewModel { idConstituency = 2, constituencyName = "Okręg 2" }
                    },
                    FilterListWrapper = new FilterListWrapper
                    {
                        VoivodeshipFilter = new List<VoivodeshipViewModel>
                        {
                            new VoivodeshipViewModel { idVoivodeship = 1, voivodeshipName = "Województwo 1" }
                        },
                        CountyFilter = new List<CountyViewModel>
                        {
                            new CountyViewModel { IdCounty = 1, CountyName = "Powiat 1" }
                        },
                        ProvinceFilter = new List<ProvinceViewModel>
                        {
                            new ProvinceViewModel { IdProvince = 1, ProvinceName = "Gmina 1" }
                        }
                    }
                });

            _constituencyManagementServiceMock = new Mock<IConstituencyManagementService>();
            _constituencyManagementServiceMock
                .Setup(service => service.Constituences())
                .ReturnsAsync(new List<ConstituencyViewModel>
                {
                    new ConstituencyViewModel { idConstituency = 1, constituencyName = "Okręg 1" },
                    new ConstituencyViewModel { idConstituency = 2, constituencyName = "Okręg 2" }
                });

            _districtManagementServiceMock = new Mock<IDistrictManagementService>();

            Services.AddSingleton(_filterWrapperServiceMock.Object);
            Services.AddSingleton(_constituencyManagementServiceMock.Object);
            Services.AddSingleton(_districtManagementServiceMock.Object);

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
        public void DistrictList_Should_Render_Correctly_For_Authorized_User()
        {
            // Act
            var cut = RenderComponent<DistrictList>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Console.WriteLine(cut.Markup);
                Assert.Contains("KONFIGURACJA OBWODÓW WYBORCZYCH", cut.Markup);
                Assert.Contains("Dodaj Obwód", cut.Markup);
                Assert.Contains("Okręg", cut.Markup);
                Assert.Contains("Województwo", cut.Markup);
                Assert.Contains("Powiat", cut.Markup);
                Assert.Contains("Gmina", cut.Markup);
                Assert.Contains("Nazwa obwodu", cut.Markup);
                Assert.Contains("Siedziba", cut.Markup);
                Assert.Contains("Nazwa okręgu", cut.Markup);
                Assert.Contains("Opcje konfiguracji", cut.Markup);

            });
        }

        [Fact]
        public void DistrictList_Should_Render_NotAuthorized_For_Unauthorized_User()
        {
            // Arrange
            var unauthorizedAuthState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(unauthorizedAuthState));

            // Act
            var cut = RenderComponent<DistrictList>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nie posiadasz odpowiednich uprawnień do wyświetlenia tej strony", cut.Markup);
                Assert.Contains("WarningIcon.png", cut.Markup);
            });
        }

        [Fact]
        public void DistrictList_Should_Navigate_To_AddDistrict_On_ButtonClick()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<DistrictList>();
            var addButton = cut.Find("button.add-button");
            addButton.Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.EndsWith("/adddistrict", navigationManager.Uri);
            });
        }
        [Fact]
        public async Task DistrictList_Should_Display_Link_To_Modify_And_Delete()
        {
            // Arrange
            var cut = RenderComponent<DistrictList>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Console.WriteLine(cut.Markup);
                Assert.Contains("/modifydistrict/1", cut.Markup);
                Assert.Contains("/deletedistrict/1", cut.Markup);
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
