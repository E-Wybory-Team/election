using Bunit;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Moq;
using E_Wybory.Client.Components.Pages;
using E_Wybory.Client.Services;
using E_Wybory.Client.ViewModels;
using Microsoft.AspNetCore.Components;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class ElectionModifyTests : TestContext
    {
        private readonly Mock<IElectionManagementService> _electionManagementServiceMock;
        private readonly Mock<IElectionTypeManagementService> _electionTypeManagementServiceMock;

        public ElectionModifyTests()
        {
            _electionManagementServiceMock = new Mock<IElectionManagementService>();
            _electionManagementServiceMock
                .Setup(service => service.Elections())
                .ReturnsAsync(new List<ElectionViewModel>
                {
                    new ElectionViewModel { IdElection = 1, IdElectionType = 1, ElectionTour = 1, ElectionStartDate = new DateTime(2023, 1, 1), ElectionEndDate = new DateTime(2023, 1, 2) },
                });

            _electionManagementServiceMock
                .Setup(service => service.GetElectionById(It.IsAny<int>()))
                .ReturnsAsync(new ElectionViewModel
                {
                    IdElection = 1,
                    IdElectionType = 1,
                    ElectionTour = 1,
                    ElectionStartDate = new DateTime(2023, 1, 1),
                    ElectionEndDate = new DateTime(2023, 1, 2)
                });

            _electionManagementServiceMock
                .Setup(service => service.PutElection(It.IsAny<ElectionViewModel>()))
                .ReturnsAsync(true);

            Services.AddSingleton(_electionManagementServiceMock.Object);

            _electionTypeManagementServiceMock = new Mock<IElectionTypeManagementService>();
            _electionTypeManagementServiceMock
                .Setup(service => service.ElectionTypes())
                .ReturnsAsync(new List<ElectionTypeViewModel>
                {
                    new ElectionTypeViewModel { IdElectionType = 1, ElectionTypeName = "Prezydenckie" }
                });

            Services.AddSingleton(_electionTypeManagementServiceMock.Object);

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
        public void ElectionModify_Should_Render_Correctly_For_Authorized_User()
        {
            // Act
            var cut = RenderComponent<ElectionModify>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("MODYFIKOWANIE WYBORÓW", cut.Markup);
                Assert.Contains("WYBORY DO EDYCJI", cut.Markup);
                Assert.Contains("RODZAJ", cut.Markup);
                Assert.Contains("ROZPOCZĘCIE WYBORÓW", cut.Markup);
                Assert.Contains("KONIEC WYBORÓW", cut.Markup);
                Assert.Contains("TURA WYBORÓW", cut.Markup);
                Assert.Contains("MODYFIKUJ WYBORY", cut.Markup);
                Assert.Contains("ANULUJ", cut.Markup);
            });
        }

        [Fact]
        public void ElectionModify_Should_Render_NotAuthorized_For_Unauthorized_User()
        {
            // Arrange
            var unauthorizedAuthState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(unauthorizedAuthState));

            // Act
            var cut = RenderComponent<ElectionModify>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nie posiadasz odpowiednich uprawnień do wyświetlenia tej strony", cut.Markup);
                Assert.Contains("WarningIcon.png", cut.Markup);
            });
        }

        [Fact]
        public void ElectionModify_Should_Call_PutElection_On_ValidSubmission()
        {
            // Arrange
            var cut = RenderComponent<ElectionModify>();
            _electionManagementServiceMock
               .Setup(service => service.ElectionOfTypeAtTimeExist(It.IsAny<ElectionViewModel>()))
               .ReturnsAsync(true);

            // Act
            var startDateInput = cut.Find("input#dzien-wyborow");
            var endDateInput = cut.Find("input#dzien-wyborow-koniec");
            var tourInput = cut.Find("input#tura");
            var select = cut.Find("select#currentElection");
            var selectElectionType = cut.Find("select#rodzaj");

            select.Change("1");
            selectElectionType.Change("1");
            startDateInput.Change("2023-01-01T10:00");
            endDateInput.Change("2023-01-02T18:00");
            tourInput.Change("2");

            var submitButton = cut.Find("button.submit-button");
            submitButton.Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Console.WriteLine(cut.Markup);
                Assert.Contains("Zmodyfikowano wybory pomyślnie!", cut.Markup);

            });
        }

        [Fact]
        public void ElectionModify_Should_Navigate_To_ConfigureElection_On_Cancel()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<ElectionModify>();
            var cancelButton = cut.Find("button.cancel-button");
            cancelButton.Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.EndsWith("/configure-election", navigationManager.Uri);
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
