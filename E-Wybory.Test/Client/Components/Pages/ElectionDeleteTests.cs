﻿using Bunit;
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
    public class ElectionDeleteTests : TestContext
    {
        private readonly Mock<IElectionManagementService> _electionManagementServiceMock;
        private readonly Mock<IElectionTypeManagementService> _electionTypeManagementServiceMock;

        public ElectionDeleteTests()
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
                .Setup(service => service.DeleteElection(It.IsAny<int>()))
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
        public void ElectionDelete_Should_Render_Correctly_For_Authorized_User()
        {
            // Act
            var cut = RenderComponent<ElectionDelete>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("POTWIERDŹ USUNIĘCIE WYBORÓW", cut.Markup);
                Assert.Contains("WYBORY", cut.Markup);
                Assert.Contains("USUŃ WYBORY", cut.Markup);
                Assert.Contains("ANULUJ", cut.Markup);
            });
        }

        [Fact]
        public void ElectionDelete_Should_Render_NotAuthorized_For_Unauthorized_User()
        {
            // Arrange
            var unauthorizedAuthState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(unauthorizedAuthState));

            // Act
            var cut = RenderComponent<ElectionDelete>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nie posiadasz odpowiednich uprawnień do wyświetlenia tej strony", cut.Markup);
                Assert.Contains("WarningIcon.png", cut.Markup);
            });
        }

        [Fact]
        public async Task ElectionDelete_Should_Fill_Inputs_After_Election_Selection()
        {
            // Arrange
            var cut = RenderComponent<ElectionDelete>();
            var select = cut.Find("select");

            // Act
            await cut.InvokeAsync(() => select.Change("1"));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Prezydenckie", cut.Markup);
                Assert.Contains("2023-01-01", cut.Markup);
                Assert.Contains("2023-01-02", cut.Markup);
                Assert.Contains("Tura: 1", cut.Markup);
            });
        }

        [Fact]
        public void ElectionDelete_Should_Display_Error_When_Candidates_Assigned()
        {
            // Arrange
            _electionManagementServiceMock
                .Setup(service => service.ElectionIsNotSetToCandidate(It.IsAny<int>()))
                .ReturnsAsync(false); 

            var cut = RenderComponent<ElectionDelete>();
            var select = cut.Find("select#currentElection");
            var form = cut.Find("form");

            // Act
            select.Change("1");
            form.Submit(); 

            // Assert
            cut.WaitForAssertion(() =>
            {
                Console.WriteLine(cut.Markup);
                Assert.Contains("Podane wybory są przypisane do min. 1 kandydata. Należy najpierw zmienić im przypisane wybory!", cut.Markup);
            });

            _electionManagementServiceMock.Verify(service => service.ElectionIsNotSetToCandidate(It.IsAny<int>()), Times.Once);
            _electionManagementServiceMock.Verify(service => service.DeleteElection(It.IsAny<int>()), Times.Never); 
        }

        [Fact]
        public void ElectionDelete_Should_Call_DeleteElection_When_No_Candidates_Assigned()
        {
            // Arrange
            _electionManagementServiceMock
                .Setup(service => service.ElectionIsNotSetToCandidate(It.IsAny<int>()))
                .ReturnsAsync(true);

            _electionManagementServiceMock
                .Setup(service => service.DeleteElection(It.IsAny<int>()))
                .ReturnsAsync(true);

            var cut = RenderComponent<ElectionDelete>();
            var select = cut.Find("select#currentElection");
            var form = cut.Find("form");

            // Act
            select.Change("1");
            form.Submit();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Console.WriteLine(cut.Markup);
                Assert.Contains("Usunięto wybory pomyślnie!", cut.Markup);
            });

            _electionManagementServiceMock.Verify(service => service.DeleteElection(It.IsAny<int>()), Times.Once);
            _electionManagementServiceMock.Verify(service => service.ElectionIsNotSetToCandidate(It.IsAny<int>()), Times.Once);
        }



        [Fact]
        public void ElectionDelete_Should_Navigate_To_ConfigureElection_On_Cancel()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();

            // Act
            var cut = RenderComponent<ElectionDelete>();
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
