using E_Wybory.Client.Policies;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace E_Wybory.Test.Client.Policies
{
    public class ElectionPasswordPolicyAttributeTests
    {
        [Theory]
        [InlineData("Password1!", true)]
        [InlineData("password1!", false)]
        [InlineData("PASSWORD1!", false)]
        [InlineData("Password!", false)]
        [InlineData("Password1", false)]
        [InlineData("P@ssw0rd", true)]
        public void IsValid_ShouldValidatePasswordCorrectly(string password, bool expectedIsValid)
        {
            // Arrange
            var attribute = new ElectionPasswordPolicyAttribute();

            // Act
            var result = attribute.GetValidationResult(password, new ValidationContext(new object()));

            // Assert
            if (expectedIsValid)
            {
                Assert.Equal(ValidationResult.Success, result);
            }
            else
            {
                Assert.NotEqual(ValidationResult.Success, result);
            }
        }
    }
}
