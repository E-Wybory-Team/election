using Bunit;
using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using E_Wybory.Client.ViewModels;
using E_Wybory.Client.Services;
using E_Wybory.Client.Components.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class RegisterTests : TestContext
    {
        private readonly Mock<IDistrictManagementService> _districtManagementServiceMock;
        private readonly Mock<IPersonManagementService> _personManagementServiceMock;
        private readonly Mock<IElectionUserManagementService> _electionUserManagementServiceMock;
        private readonly Mock<IAuthService> _authServiceMock;

        public RegisterTests()
        {
            _districtManagementServiceMock = new Mock<IDistrictManagementService>();
            _personManagementServiceMock = new Mock<IPersonManagementService>();
            _electionUserManagementServiceMock = new Mock<IElectionUserManagementService>();
            _authServiceMock = new Mock<IAuthService>();

            Services.AddSingleton(_districtManagementServiceMock.Object);
            Services.AddSingleton(_personManagementServiceMock.Object);
            Services.AddSingleton(_electionUserManagementServiceMock.Object);
            Services.AddSingleton(_authServiceMock.Object);

            var notLoggedInAuthState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider(notLoggedInAuthState));
            Services.AddAuthorizationCore();

            Services.AddSingleton<IAuthorizationPolicyProvider, DefaultAuthorizationPolicyProvider>();
            Services.AddSingleton<IAuthorizationService, DefaultAuthorizationService>();
        }

        [Theory]
        [InlineData("", "Kowalski", "44051401359", "1944-05-14", "jan.kowalski@example.com", "123456789", "password123", "password123", 1, "Imię jest obowiązkowe!")]
        [InlineData("Jan", "", "44051401359", "1944-05-14", "jan.kowalski@example.com", "123456789", "password123", "password123", 1, "Nazwisko jest obowiązkowe!")]
        [InlineData("Jan", "Kowalski", "", "1944-05-14", "jan.kowalski@example.com", "123456789", "password123", "password123", 1, "PESEL jest obowiązkowy!")]
        [InlineData("Jan", "Kowalski", "44051401359", "1944-05-14", "", "123456789", "password123", "password123", 1, "Adres email jest obowiązkowy!")]
        [InlineData("Jan", "Kowalski", "44051401359", "1944-05-14", "jan.kowalski@example.com", "", "password123", "password123", 1, "Numer telefonu jest obowiązkowy!")]
        [InlineData("Jan", "Kowalski", "44051401359", "1944-05-14", "jan.kowalski@example.com", "123456789", "", "password123", 1, "Hasło jest obowiązkowe!")]
        [InlineData("Jan", "Kowalski", "44051401359", "1944-05-14", "jan.kowalski@example.com", "123456789", "password123", "", 1, "Hasło (potwierdzenie) jest obowiązkowe!")]
        public async Task RegisterForm_Should_Validate_Each_Field(
            string firstName,
            string lastName,
            string pesel,
            string dateOfBirthString,
            string email,
            string phoneNumber,
            string password,
            string confirmPassword,
            int selectedDistrictId,
            string expectedErrorMessage)
        {

           
            // Arrange
            var districts = new List<DistrictViewModel>
            {
                new DistrictViewModel { IdDistrict = 1, DistrictName = "District 1", DistrictHeadquarters = "HQ 1" }
            };

            _districtManagementServiceMock.Setup(service => service.Districts()).ReturnsAsync(districts);

            var cut = RenderComponent<Register>();

            // Act
            var model = cut.Instance.GetRegisterModel();
            await cut.InvokeAsync(() => model.FirstName = firstName);
            await cut.InvokeAsync(() => model.LastName = lastName);
            await cut.InvokeAsync(() => model.PESEL = pesel);
            await cut.InvokeAsync(() => model.DateOfBirthString = dateOfBirthString);
            await cut.InvokeAsync(() => model.Email = email);
            await cut.InvokeAsync(() => model.PhoneNumber = phoneNumber);
            await cut.InvokeAsync(() => model.Password = password);
            await cut.InvokeAsync(() => model.ConfirmPassword = confirmPassword);
            await cut.InvokeAsync(() => model.SelectedDistrictId = selectedDistrictId);

            Console.WriteLine(cut.Markup);
            var submitButton = cut.Find("button.submit-button");
            submitButton.Click();
            cut.WaitForAssertion(() => cut.Markup.Contains(expectedErrorMessage), TimeSpan.FromSeconds(5));

            // Assert
            
            Assert.Contains(expectedErrorMessage, cut.Markup);
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
