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
using E_Wybory.Client.ViewModels;
using System.Collections.Generic;
using E_Wybory.Client.FilterData;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class CommissionersAddTests : TestContext
    {
        private readonly Mock<IVoivodeshipManagementService> _voivodeshipManagementServiceMock;
        private readonly Mock<IUserTypeManagementService> _userTypeManagementServiceMock;
        private readonly Mock<IUserTypeSetsManagementService> _userTypeSetsManagementServiceMock;
        private readonly Mock<IFilterWrapperManagementService> _filterWrapperServiceMock;
        private readonly Mock<IPersonManagementService> _personManagementServiceMock;
        private readonly Mock<IElectionUserManagementService> _electionUserManagementServiceMock;

        public CommissionersAddTests()
        {
            _voivodeshipManagementServiceMock = new Mock<IVoivodeshipManagementService>();
            _userTypeManagementServiceMock = new Mock<IUserTypeManagementService>();
            _userTypeSetsManagementServiceMock = new Mock<IUserTypeSetsManagementService>();
            _filterWrapperServiceMock = new Mock<IFilterWrapperManagementService>();
            _personManagementServiceMock = new Mock<IPersonManagementService>();
            _electionUserManagementServiceMock = new Mock<IElectionUserManagementService>();

            _voivodeshipManagementServiceMock
                .Setup(service => service.Voivodeships())
                .ReturnsAsync(new List<VoivodeshipViewModel>
                {
                    new VoivodeshipViewModel { idVoivodeship = 1, voivodeshipName = "Mazowieckie" },
                    new VoivodeshipViewModel { idVoivodeship = 2, voivodeshipName = "Pomorskie" }
                });

            _userTypeManagementServiceMock
                .Setup(service => service.GetUserTypesOfGroup(It.IsAny<int>()))
                .ReturnsAsync(new List<UserTypeViewModel>
                {
                    new UserTypeViewModel { IdUserType = 1, UserTypeName = "Przewodniczący" },
                    new UserTypeViewModel { IdUserType = 2, UserTypeName = "Członek" }
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

            _personManagementServiceMock
                .Setup(service => service.GetPersonIdByPeselAsync(It.IsAny<string>()))
                .ReturnsAsync(1);

            _electionUserManagementServiceMock
                .Setup(service => service.GetElectionUserByPersonId(1))
                .ReturnsAsync(new ElectionUserViewModel { IdElectionUser = 1, IdDistrict = 1 });

            _userTypeSetsManagementServiceMock
                .Setup(service => service.UserWithTypeGroupExists(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            _userTypeSetsManagementServiceMock
                .Setup(service => service.AddUserTypeSet(It.IsAny<UserTypeSetViewModel>()))
                .ReturnsAsync(true);
        

            Services.AddSingleton(_voivodeshipManagementServiceMock.Object);
            Services.AddSingleton(_userTypeManagementServiceMock.Object);
            Services.AddSingleton(_userTypeSetsManagementServiceMock.Object);
            Services.AddSingleton(_filterWrapperServiceMock.Object);
            Services.AddSingleton(_personManagementServiceMock.Object);
            Services.AddSingleton(_electionUserManagementServiceMock.Object);

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
        public void CommissionersAdd_Should_Render_Correctly_For_Authorized_User()
        {
            // Act
            var cut = RenderComponent<CommissionersAdd>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("DODAWANIE CZŁONKÓW KOMISJI DO OBWODU", cut.Markup);
                Assert.Contains("PESEL CZŁONKA KOMISJI", cut.Markup);
                Assert.Contains("WOJEWÓDZTWO", cut.Markup);
                Assert.Contains("POWIAT", cut.Markup);
                Assert.Contains("GMINA", cut.Markup);
                Assert.Contains("NUMER OBWODU", cut.Markup);
                Assert.Contains("STOPIEŃ", cut.Markup);
                Assert.Contains("DODAJ", cut.Markup);
                Assert.Contains("ANULUJ", cut.Markup);
            });
        }

        [Fact]
        public void CommissionersAdd_Should_Render_NotAuthorized_For_Unauthorized_User()
        {
            // Arrange
            var unauthorizedAuthState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(unauthorizedAuthState));

            // Act
            var cut = RenderComponent<CommissionersAdd>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nie posiadasz odpowiednich uprawnień do wyświetlenia tej strony", cut.Markup);
                Assert.Contains("WarningIcon.png", cut.Markup);
            });
        }

        [Fact]
        public async Task CommissionersAdd_Should_Display_Validation_Errors_On_Invalid_Form_Submission()
        {
            // Act
            var cut = RenderComponent<CommissionersAdd>();

            var submitButton = cut.Find("button.submit-button");
            await cut.InvokeAsync(() => submitButton.Click());

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nie uzupełniono wszystkich pól!", cut.Markup);
            });
        }

        [Fact]
        public async Task CommissionersAdd_Should_Add_Commissioner_With_Valid_Data()
        {
            

            // Arrange
            var cut = RenderComponent<CommissionersAdd>();

            var peselInput = cut.Find("input#pesel");
            await cut.InvokeAsync(() => peselInput.Change("12345678901"));

            var voivodeshipSelect = cut.Find("select#wojewodztwo");
            await cut.InvokeAsync(() => voivodeshipSelect.Change("1"));

            var countySelect = cut.Find("select#powiat");
            await cut.InvokeAsync(() => countySelect.Change("1"));

            var provinceSelect = cut.Find("select#gmina");
            await cut.InvokeAsync(() => provinceSelect.Change("1"));

            var districtSelect = cut.Find("select#numer-obwodu");
            await cut.InvokeAsync(() => districtSelect.Change("1"));

            var userTypeSelect = cut.Find("select#stopien");
            await cut.InvokeAsync(() => userTypeSelect.Change("1"));

            // Act
            var submitButton = cut.Find("button.submit-button");
            await cut.InvokeAsync(() => submitButton.Click());

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Dodano członka komisji pomyślnie!", cut.Markup);
            });
        }

        [Fact]
        public void CommissionersAdd_Should_Display_Regions_Correctly()
        {
            // Act
            var cut = RenderComponent<CommissionersAdd>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Mazowieckie", cut.Markup);
                Assert.Contains("Pomorskie", cut.Markup);
            });
        }
        [Fact]
        public async Task CommissionersAdd_Should_Navigate_To_CommissionersList_On_Cancel()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<CommissionersAdd>();
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
