using Bunit;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using E_Wybory.Client.Components.Pages;
using E_Wybory.Client.Services;
using E_Wybory.Client.ViewModels;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class ForgotPasswordTests : TestContext
    {
        private readonly Mock<IAuthService> _authServiceMock;

        public ForgotPasswordTests()
        {
            _authServiceMock = new Mock<IAuthService>();

            _authServiceMock
                .Setup(service => service.ForgetPassword(It.IsAny<ForgetPasswordViewModel>()))
                .ReturnsAsync(true);

            Services.AddSingleton(_authServiceMock.Object);
        }

        [Fact]
        public void ForgotPassword_Should_Display_Error_On_Invalid_Email()
        {
            // Arrange
            var cut = RenderComponent<ForgotPassword>();
            var input = cut.Find("input");

            // Act
            input.Change("invalid-email");
            var form = cut.Find("form");
            form.Submit();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nieprawidłowy adres e-mail.", cut.Markup);
            });
        }

        [Fact]
        public void ForgotPassword_Should_Display_Error_On_Empty_Email()
        {
            // Arrange
            var cut = RenderComponent<ForgotPassword>();
            var form = cut.Find("form");

            // Act
            form.Submit();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Adres e-mail jest obowiązkowy.", cut.Markup);
            });
        }

        [Fact]
        public async Task ForgotPassword_Should_Display_Success_Message_On_Valid_Email()
        {
            // Arrange
            var cut = RenderComponent<ForgotPassword>();
            var input = cut.Find("input");

            // Act
            input.Change("valid-email@example.com");
            var form = cut.Find("form");
            form.Submit();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Kod do resetu hasła został wysłany na podany adres e-mail", cut.Markup);
            });

            _authServiceMock.Verify(service => service.ForgetPassword(It.IsAny<ForgetPasswordViewModel>()), Times.Once);
        }
    }
}
