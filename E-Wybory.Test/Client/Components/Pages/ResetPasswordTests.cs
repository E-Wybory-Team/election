using Bunit;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using E_Wybory.Client.Components.Pages;
namespace E_Wybory.Test.Client.Components.Pages
{
    public class ResetPasswordTests : TestContext
    {
        [Fact]
        public void ResetPassword_Should_Display_Validation_Error_When_Fields_Are_Empty()
        {
            // Arrange
            var cut = RenderComponent<ResetPassword>();

            // Act
            var submitButton = cut.Find("button.submit-button");
            submitButton.Click();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Nowe hasło jest wymagane.", cut.Markup);
                Assert.Contains("Potwierdzenie hasła jest wymagane.", cut.Markup);
            });
        }

    }
}
