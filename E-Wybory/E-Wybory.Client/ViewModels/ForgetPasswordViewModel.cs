using E_Wybory.Client.Policies;
using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class ForgetPasswordViewModel
    {
        [Required(ErrorMessage = "Podaj adres email."), EmailAddress(ErrorMessage = "Podaj poprawny adres email")]

        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Podaj hasło")]
        [ElectionPasswordPolicy]
        public string NewPassword { get; set; } = String.Empty;

        [Required, Compare(nameof(NewPassword), ErrorMessage = "Hasła muszą się zgadzać")]
        public string NewConfirmPassword { get; set; } = String.Empty;

        [Required(ErrorMessage = "Podaj kod weryfikacyjny")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Kod weryfikacyjny musi składać się z 6 cyfr.")]
        [DataType(DataType.Text)]
        [Display(Name = "Kod weryfikacyjny")]
        public string ResetCode { get; set; } = string.Empty;


        [Required(ErrorMessage = "Podaj adres email."), EmailAddress(ErrorMessage = "Podaj poprawny adres email")]
        public string Email { get; set; } = string.Empty;
    }
}
