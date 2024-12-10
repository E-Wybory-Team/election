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
using E_Wybory.Client.FilterData;
using E_Wybory.Client.ViewModels;

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
            _voivodeshipManagementServiceMock
                .Setup(service => service.Voivodeships())
                .ReturnsAsync(new List<VoivodeshipViewModel>
                {
                    new VoivodeshipViewModel { idVoivodeship = 1, voivodeshipName = "Mazowieckie" },
                    new VoivodeshipViewModel { idVoivodeship = 2, voivodeshipName = "Pomorskie" }
                });

            _userTypeSetsManagementServiceMock
                .Setup(service => service.CommissionersOfDistrict(It.IsAny<int>()))
                .ReturnsAsync(new List<CommissionerViewModel>
                {
                    new CommissionerViewModel
                    {
                        personViewModel = new PersonViewModel { Name = "Jan", Surname = "Kowalski", PESEL = "12345678901" },
                        userViewModel = new ElectionUserViewModel { PhoneNumber = "+48123456789", Email = "jan.kowalski@example.com", IdElectionUser = 1 },
                        userType = new UserTypeViewModel { UserTypeName = "Przewodniczący" }
                    },
                    new CommissionerViewModel
                    {
                        personViewModel = new PersonViewModel { Name = "Anna", Surname = "Nowak", PESEL = "98765432109" },
                        userViewModel = new ElectionUserViewModel { PhoneNumber = "+48987654321", Email = "anna.nowak@example.com", IdElectionUser = 2 },
                        userType = new UserTypeViewModel { UserTypeName = "Członek" }
                    }
                });



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
        public async Task CommissionersList_Should_Display_Filtered_Commissioners()
        {

            // Arrange
            var cut = RenderComponent<CommissionersList>();
            Console.WriteLine(cut.Markup);

            // Act
            var voivodeshipSelect = cut.Find("select#wojewodztwo"); 
            await cut.InvokeAsync(() => voivodeshipSelect.Change("1"));

            var countySelect = cut.Find("select#powiat");
            await cut.InvokeAsync(() => countySelect.Change("1"));

            var provinceSelect = cut.Find("select#gmina"); 
            await cut.InvokeAsync(() => provinceSelect.Change("1")); 

            var districtSelect = cut.Find("select#numer-obwodu"); 
            await cut.InvokeAsync(() => districtSelect.Change("1")); 

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Jan", cut.Markup);
                Assert.Contains("Kowalski", cut.Markup);
                Assert.Contains("12345678901", cut.Markup);
                Assert.Contains("+48123456789", cut.Markup);
                Assert.Contains("jan.kowalski@example.com", cut.Markup);
                Assert.Contains("Przewodniczący", cut.Markup);

                Assert.Contains("Anna", cut.Markup);
                Assert.Contains("Nowak", cut.Markup);
                Assert.Contains("98765432109", cut.Markup);
                Assert.Contains("+48987654321", cut.Markup);
                Assert.Contains("anna.nowak@example.com", cut.Markup);
                Assert.Contains("Członek", cut.Markup);
            });
        }

        [Fact]
        public async Task CommissionersList_Should_Display_Link_To_Modify_And_Delete()
        {
            // Arrange
            var cut = RenderComponent<CommissionersList>();
            var voivodeshipSelect = cut.Find("select#wojewodztwo");
            await cut.InvokeAsync(() => voivodeshipSelect.Change("1"));

            var countySelect = cut.Find("select#powiat");
            await cut.InvokeAsync(() => countySelect.Change("1"));

            var provinceSelect = cut.Find("select#gmina");
            await cut.InvokeAsync(() => provinceSelect.Change("1"));

            var districtSelect = cut.Find("select#numer-obwodu");
            await cut.InvokeAsync(() => districtSelect.Change("1"));
            // Assert
            cut.WaitForAssertion(() =>
            {
                Console.WriteLine(cut.Markup);
                Assert.Contains("/commissionersmodify/1", cut.Markup);
                Assert.Contains("/commissionersdelete/1", cut.Markup);
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
