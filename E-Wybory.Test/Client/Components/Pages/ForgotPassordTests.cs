using Bunit;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using E_Wybory.Client.Components.Pages;

namespace E_Wybory.Test.Client.Components.Pages
{
    public class ForgotPasswordTests : TestContext
    {
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
                Assert.Contains("Adres e-mail jest wymagany.", cut.Markup);
            });
        }

    }
}
