using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace E_Wybory.Client.Validators
{
    public class ElectionPasswordPolicyAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var password = value as string;

            if (string.IsNullOrEmpty(password))
            {
                return new ValidationResult("Hasło jest wymagane.");
            }

            if (password.Length < 8)
            {
                return new ValidationResult("Hasło musi mieć co najmniej 8 znaków.");
            }

            if (!password.Any(char.IsUpper))
            {
                return new ValidationResult("Hasło musi zawierać co najmniej jedną wielką literę.");
            }

            if (!password.Any(char.IsLower))
            {
                return new ValidationResult("Hasło musi zawierać co najmniej jedną małą literę.");
            }

            if (!password.Any(char.IsDigit))
            {
                return new ValidationResult("Hasło musi zawierać co najmniej jedną cyfrę.");
            }

            //if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            //{
            //    return new ValidationResult("Hasło musi zawierać co najmniej jeden znak specjalny.");
            //}

            var specialCharacters = "!@#$%^&*()_+-=[]{}|;:'\",.<>?/`~";
            if (!password.Any(ch => specialCharacters.Contains(ch)))
            {
                return new ValidationResult("Hasło musi zawierać co najmniej jeden znak specjalny.");
            }

            // Add more custom rules as needed

            return ValidationResult.Success!;
        }
    }
}
