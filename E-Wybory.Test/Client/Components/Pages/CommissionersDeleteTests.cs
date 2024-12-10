using Bunit;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using E_Wybory.Client.Components.Pages;
using Moq;
using E_Wybory.Client.Services;
using E_Wybory.Client.ViewModels;
using E_Wybory.Client.FilterData;
using Microsoft.AspNetCore.Components;
using E_Wybory.Domain.Entities;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class CommissionersDeleteTests : TestContext
    {
        private readonly Mock<IElectionUserManagementService> _electionUserManagementServiceMock;
        private readonly Mock<IPersonManagementService> _personManagementServiceMock;
        private readonly Mock<IUserTypeSetsManagementService> _userTypeSetsManagementServiceMock;
        private readonly Mock<IUserTypeManagementService> _userTypeManagementServiceMock;
        private readonly Mock<IFilterWrapperManagementService> _filterWrapperServiceMock;
        private readonly Mock<IVoivodeshipManagementService> _voivodeshipManagementServiceMock;

        public CommissionersDeleteTests()
        {
            _electionUserManagementServiceMock = new Mock<IElectionUserManagementService>();
            _personManagementServiceMock = new Mock<IPersonManagementService>();
            _userTypeSetsManagementServiceMock = new Mock<IUserTypeSetsManagementService>();
            _userTypeManagementServiceMock = new Mock<IUserTypeManagementService>();
            _filterWrapperServiceMock = new Mock<IFilterWrapperManagementService>();
            _voivodeshipManagementServiceMock = new Mock<IVoivodeshipManagementService>();

            _voivodeshipManagementServiceMock
               .Setup(service => service.Voivodeships())
               .ReturnsAsync(new List<VoivodeshipViewModel>
               {
                    new VoivodeshipViewModel { idVoivodeship = 1, voivodeshipName = "Mazowieckie" },
                    new VoivodeshipViewModel { idVoivodeship = 2, voivodeshipName = "Pomorskie" }
               });

            _filterWrapperServiceMock.Setup(service => service.GetFilteredListsWrapper(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(new FilterListWrapper
                {
                    VoivodeshipFilter = new List<VoivodeshipViewModel>
                    {
                        new VoivodeshipViewModel { idVoivodeship = 1, voivodeshipName = "Mazowieckie" },
                        new VoivodeshipViewModel { idVoivodeship = 2, voivodeshipName = "Pomorskie" }
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

            _electionUserManagementServiceMock
                .Setup(service => service.GetElectionUserById(It.IsAny<int>()))
                .ReturnsAsync(new ElectionUserViewModel
                {
                    IdElectionUser = 1,
                    IdDistrict = 1
                });

            _electionUserManagementServiceMock
                .Setup(service => service.UserExists(It.IsAny<int>()))
                .ReturnsAsync(true);

            _userTypeSetsManagementServiceMock
                .Setup(service => service.SetWithTypeGroupExists(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new UserTypeSetViewModel
                {
                    IdUserTypeSet = 1,
                    IdUserType = 1
                });

            _userTypeSetsManagementServiceMock
                .Setup(service => service.DeleteUserTypeSet(It.IsAny<int>()))
                .ReturnsAsync(true);

            _userTypeManagementServiceMock
                .Setup(service => service.GetUserTypeNameById(It.IsAny<int>()))
                .ReturnsAsync("Przewodniczący");

            _personManagementServiceMock
                .Setup(service => service.GetPersonIdByIdElectionUser(It.IsAny<int>()))
                .ReturnsAsync(new PersonViewModel
                {
                    PESEL = "12345678901"
                });

            _filterWrapperServiceMock
                .Setup(service => service.GetRegionsOfDistrict(It.IsAny<int>()))
                .ReturnsAsync(new List<string> { "Obwód 1", "Gmina 1", "Powiat 1", "Województwo 1" });


            Services.AddSingleton(_electionUserManagementServiceMock.Object);
            Services.AddSingleton(_personManagementServiceMock.Object);
            Services.AddSingleton(_userTypeSetsManagementServiceMock.Object);
            Services.AddSingleton(_userTypeManagementServiceMock.Object);
            Services.AddSingleton(_filterWrapperServiceMock.Object);
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
        public void CommissionersDelete_Should_Render_Correctly_For_Authorized_User()
        {
            // Arrange
            var cut = RenderComponent<CommissionersDelete>(parameters => parameters.Add(p => p.commissionerId, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("POTWIERDŹ USUNIĘCIE CZŁONKA KOMISJI", cut.Markup);
                Assert.Contains("12345678901", cut.Markup);
                Assert.Contains("Przewodniczący", cut.Markup);
                Assert.Contains("Obwód 1", cut.Markup);
                Assert.Contains("Gmina 1", cut.Markup);
                Assert.Contains("Powiat 1", cut.Markup);
                Assert.Contains("Województwo 1", cut.Markup);
            }, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task CommissionersDelete_Should_Display_Error_When_User_Not_Found()
        {
            // Arrange
            _electionUserManagementServiceMock
                .Setup(service => service.UserExists(It.IsAny<int>()))
                .ReturnsAsync(false);

            var cut = RenderComponent<CommissionersDelete>(parameters => parameters.Add(p => p.commissionerId, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nie znaleziono użytkownika o podanym ID", cut.Markup);
            });
        }

        [Fact]
        public async Task CommissionersDelete_Should_Delete_Commissioner_Successfully()
        {
            // Arrange
            
            var cut = RenderComponent<CommissionersDelete>(parameters => parameters.Add(p => p.commissionerId, 1));

            // Act
            var deleteButton = cut.Find("button.red-submit-button");
            await cut.InvokeAsync(() => deleteButton.Click());

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Usunięto członka komisji pomyślnie!", cut.Markup);
            });
        }

        [Fact]
        public async Task CommissionersDelete_Should_Navigate_To_CommissionersList_On_Cancel()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<CommissionersDelete>(parameters => parameters.Add(p => p.commissionerId, 1)); 
            var cancelButton = cut.Find("button.cancel-button");
            cancelButton.Click();

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
