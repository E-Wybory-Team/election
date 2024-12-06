﻿using Bunit;
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
using E_Wybory.Client.FilterData;

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
            _userTypeManagementServiceMock
                .Setup(service => service.GetUserTypesOfGroup(It.IsAny<int>()))
                .ReturnsAsync(new List<UserTypeViewModel>
                {
                    new UserTypeViewModel { IdUserType = 1, UserTypeName = "Przewodniczący" },
                    new UserTypeViewModel { IdUserType = 2, UserTypeName = "Członek" }
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
        public void CommissionersModify_Should_Render_Correctly_For_Authorized_User()
        {
            // Arrange
            _userTypeSetsManagementServiceMock
                .Setup(service => service.SetWithTypeGroupExists(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new UserTypeSetViewModel
                {
                    IdUserTypeSet = 1,
                    IdUserType = 1
                });

            _userTypeManagementServiceMock
                .Setup(service => service.GetUserTypeNameById(It.IsAny<int>()))
                .ReturnsAsync("Przewodniczący");

            _personManagementServiceMock
                .Setup(service => service.GetPersonIdByIdElectionUser(It.IsAny<int>()))
                .ReturnsAsync(new PersonViewModel
                {
                    PESEL = "12345678901"
                });

            _electionUserManagementServiceMock
                .Setup(service => service.UserExists(It.IsAny<int>()))
                .ReturnsAsync(true);

            _filterWrapperServiceMock
                .Setup(service => service.GetRegionsOfDistrict(It.IsAny<int>()))
                .ReturnsAsync(new List<string> { "Obwód 1", "Gmina 1", "Powiat 1", "Województwo 1" });

            // Act
            var cut = RenderComponent<CommissionersModify>(parameters => parameters.Add(p => p.commissionerId, 1));

            var voivodeshipSelect = cut.Find("select#wojewodztwo");
            voivodeshipSelect.Change("1");

            var countySelect = cut.Find("select#powiat");
            countySelect.Change("1");

            var provinceSelect = cut.Find("select#gmina");
            provinceSelect.Change("1");

            // Assert
            cut.WaitForAssertion(() =>
            {
                Console.WriteLine(cut.Markup);
                Assert.Contains("MODYFIKOWANIE CZŁONKA KOMISJI", cut.Markup);
                Assert.Contains("PESEL CZŁONKA KOMISJI", cut.Markup);
                Assert.Contains("12345678901", cut.Markup); 
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
