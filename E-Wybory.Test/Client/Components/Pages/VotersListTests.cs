using Bunit;
using Moq;
using Xunit;
using E_Wybory.Client.ViewModels;
using E_Wybory.Client.Components.Pages;
using E_Wybory.Client.Services;
using E_Wybory.Client.FilterData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class VotersListTests : TestContext
    {
        private readonly Mock<IFilterWrapperManagementService> _filterWrapperServiceMock;
        private readonly Mock<IElectionUserManagementService> _electionUserManagementServiceMock;
        private readonly Mock<IVoterManagementService> _voterManagementServiceMock;

        public VotersListTests()
        {

            _filterWrapperServiceMock = new Mock<IFilterWrapperManagementService>();
            _electionUserManagementServiceMock = new Mock<IElectionUserManagementService>();
            _voterManagementServiceMock = new Mock<IVoterManagementService>();


            Services.AddSingleton(_filterWrapperServiceMock.Object);
            Services.AddSingleton(_electionUserManagementServiceMock.Object);
            Services.AddSingleton(_voterManagementServiceMock.Object);

            Services.AddAuthorizationCore();

            var mockAuthorizationPolicyProvider = new Mock<IAuthorizationPolicyProvider>();
            Services.AddSingleton(mockAuthorizationPolicyProvider.Object);
            Services.AddSingleton<IAuthorizationPolicyProvider, DefaultAuthorizationPolicyProvider>();
            Services.AddSingleton<IAuthorizationService, DefaultAuthorizationService>();

            var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Role, "Administratorzy"),
            }, "test"))));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(authState));


            _filterWrapperServiceMock.Setup(service => service.GetFilteredListsWrapper(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(new FilterListWrapper
                {
                    VoivodeshipFilter = new List<VoivodeshipViewModel>
                    {
                        new VoivodeshipViewModel { idVoivodeship = 1, voivodeshipName = "Województwo 1" },
                        new VoivodeshipViewModel { idVoivodeship = 2, voivodeshipName = "Województwo 2" }
                    },
                    CountyFilter = new List<CountyViewModel>
                    {
                        new CountyViewModel { IdCounty = 1, CountyName = "Powiat 1", IdVoivodeship = 1 },
                        new CountyViewModel { IdCounty = 2, CountyName = "Powiat 2", IdVoivodeship = 2 }
                    },
                    ProvinceFilter = new List<ProvinceViewModel>
                    {
                        new ProvinceViewModel { IdProvince = 1, ProvinceName = "Gmina 1", IdCounty = 1 },
                        new ProvinceViewModel { IdProvince = 2, ProvinceName = "Gmina 2", IdCounty = 2 }
                    },
                    DistrictFilter = new List<DistrictViewModel>
                    {
                        new DistrictViewModel { IdDistrict = 1, DistrictName = "Obwód 1", DistrictHeadquarters = "Siedziba 1", IdProvince = 1 },
                        new DistrictViewModel { IdDistrict = 2, DistrictName = "Obwód 2", DistrictHeadquarters = "Siedziba 2", IdProvince = 2 }
                    }
                });

            _filterWrapperServiceMock.Setup(service => service.GetFilteredUsers(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<UserPersonViewModel>
                {
                    new UserPersonViewModel
                    {
                        personViewModel = new PersonViewModel { Name = "Jan", Surname = "Kowalski", PESEL = "12345678901" },
                        userViewModel = new ElectionUserViewModel { PhoneNumber = "123456789", Email = "jan.kowalski@example.com", IdElectionUser = 1 }
                    }
                });

            _voterManagementServiceMock.Setup(service => service.GetVoterIdByElectionUserId(It.IsAny<int>()))
                .ReturnsAsync(1);
        }

        [Fact]
        public void VotersList_Should_Display_Initial_Layout()
        {
            // Arrange
            var cut = RenderComponent<VotersList>();

            // Act & Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("SPIS WYBORCÓW", cut.Markup);
                Assert.Contains("Dodaj Wyborcę", cut.Markup);
            });
        }

        [Fact]
        public async Task VotersList_Should_Display_Filtered_Users()
        {
            // Arrange
            var cut = RenderComponent<VotersList>();

            // Act
            var voivodeshipSelect = cut.Find("select");
            await cut.InvokeAsync(() => voivodeshipSelect.Change("1"));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Jan", cut.Markup);
                Assert.Contains("Kowalski", cut.Markup);
                Assert.Contains("12345678901", cut.Markup);
            });
        }

        /*[Fact]
        public async Task VotersList_Should_Filter_Regions_On_Change()
        {
            // Arrange
            var cut = RenderComponent<VotersList>();

            // Act
            var voivodeshipSelect = cut.Find("select");
            await cut.InvokeAsync(() => voivodeshipSelect.Change("1"));

            // Assert
            _filterWrapperServiceMock.Verify(service => service.GetFilteredListsWrapper(It.IsAny<int?>(), null, null), Times.Once);
            _filterWrapperServiceMock.Verify(service => service.GetFilteredUsers(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()), Times.Once);
        }*/

        [Fact]
        public async Task VotersList_Should_Display_Link_To_Modify_And_Delete()
        {
            // Arrange
            var cut = RenderComponent<VotersList>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("/modifyvoter/1", cut.Markup);
                Assert.Contains("/voterdelete/1", cut.Markup);
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
