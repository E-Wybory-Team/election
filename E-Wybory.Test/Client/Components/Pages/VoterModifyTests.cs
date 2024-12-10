using Bunit;
using Moq;
using Xunit;
using E_Wybory.Client.ViewModels;
using E_Wybory.Client.Components.Pages;
using E_Wybory.Client.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using E_Wybory.Client.FilterData;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class VoterModifyTests : TestContext
    {
        private readonly Mock<IFilterWrapperManagementService> _filterWrapperServiceMock;
        private readonly Mock<IPersonManagementService> _personManagementServiceMock;
        private readonly Mock<IElectionUserManagementService> _electionUserManagementServiceMock;
        private readonly Mock<IVoterManagementService> _voterManagementServiceMock;

        public VoterModifyTests()
        {
            _filterWrapperServiceMock = new Mock<IFilterWrapperManagementService>();
            _personManagementServiceMock = new Mock<IPersonManagementService>();
            _electionUserManagementServiceMock = new Mock<IElectionUserManagementService>();
            _voterManagementServiceMock = new Mock<IVoterManagementService>();

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

            Services.AddSingleton(_filterWrapperServiceMock.Object);
            Services.AddSingleton(_personManagementServiceMock.Object);
            Services.AddSingleton(_electionUserManagementServiceMock.Object);
            Services.AddSingleton(_voterManagementServiceMock.Object);

            var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Role, "Urzędnicy wyborczy"),
            }, "test"))));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(authState));

            Services.AddAuthorizationCore();
            Services.AddSingleton<IAuthorizationPolicyProvider, DefaultAuthorizationPolicyProvider>();
            Services.AddSingleton<IAuthorizationService, DefaultAuthorizationService>();

        }

        [Fact]
        public void VoterModify_Should_Display_Initial_Form()
        {
            // Act
            var cut = RenderComponent<VoterModify>(parameters => parameters.Add(p => p.voterId, 1));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Console.Write(cut.Markup);
                Assert.Contains("ZMIANA OBWODU WYBORCY", cut.Markup);
                Assert.Contains("PESEL WYBORCY", cut.Markup);
                Assert.Contains("WOJEWÓDZTWO", cut.Markup);
                Assert.Contains("POWIAT", cut.Markup);
                Assert.Contains("GMINA", cut.Markup);
                Assert.Contains("NUMER OBWODU", cut.Markup);
                Assert.Contains("ZATWIERDŹ ZMIANY", cut.Markup);
                Assert.Contains("ANULUJ", cut.Markup);
            });
        }

        [Fact]
        public async Task VoterModify_Should_Modify_Voter_With_Valid_Data()
        {
            // Arrange
            _personManagementServiceMock
                .Setup(service => service.GetPersonIdByPeselAsync(It.IsAny<string>()))
                .ReturnsAsync(1);

            _personManagementServiceMock
                .Setup(service => service.GetPersonById(1))
                .ReturnsAsync(new PersonViewModel { PESEL = "12345678901" });

            _electionUserManagementServiceMock
                .Setup(service => service.GetElectionUserByPersonId(1))
                .ReturnsAsync(new ElectionUserViewModel { IdElectionUser = 1 });

            _voterManagementServiceMock
                .Setup(service => service.PutVoter(It.IsAny<VoterViewModel>()))
                .ReturnsAsync(true);

            var cut = RenderComponent<VoterModify>(parameters => parameters.Add(p => p.voterId, 1));

            // Act
            var peselInput = cut.Find("input#pesel");
            peselInput.Change("12345678901");

            var submitButton = cut.Find("button[type='submit']");
            await cut.InvokeAsync(() => submitButton.Click());

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Zmodyfikowano wyborcę pomyślnie!", cut.Markup);
            });
        }

        [Fact]
        public async Task VoterModify_Should_Filter_Regions_On_Voivodeship_Change()
        {
            // Arrange
            var cut = RenderComponent<VoterModify>(parameters => parameters.Add(p => p.voterId, 1));

            // Act
            var voivodeshipSelect = cut.Find("select#wojewodztwo");
            await cut.InvokeAsync(() => voivodeshipSelect.Change("1"));

            // Assert
            _filterWrapperServiceMock.Verify(service => service.GetFilteredListsWrapper(1, null, null), Times.Once);
        }

        [Fact]
        public async Task VoterModify_Should_Navigate_To_VotersList_On_Cancel()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<VoterModify>(parameters => parameters.Add(p => p.voterId, 1));
            var cancelButton = cut.Find("button.cancel-button");
            cancelButton.Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.EndsWith("/voterslist", navigationManager.Uri);
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
