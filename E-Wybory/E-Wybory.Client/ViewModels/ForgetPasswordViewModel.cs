using E_Wybory.Client.Validators;
using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class ForgetPasswordViewModel
    {
        [Required(ErrorMessage = "Adres e-mail jest obowiązkowy."), EmailAddress(ErrorMessage = "Nieprawidłowy adres e-mail.")]
        [RegularExpression(@"^[a-zA-Z0-9._+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Nieprawidłowy adres e-mail. Adres nie może zawierać polskich znaków.")]

        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Podaj hasło.")]
        [ElectionPasswordPolicy]
        public string NewPassword { get; set; } = String.Empty;

        [Compare(nameof(NewPassword), ErrorMessage = "Hasła muszą się zgadzać.")]
        [Required(ErrorMessage = "Podaj ponownie hasło.")]
        public string NewConfirmPassword { get; set; } = String.Empty;

        [Required(ErrorMessage = "Podaj kod weryfikacyjny.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Kod weryfikacyjny musi składać się z 6 cyfr.")]
        [DataType(DataType.Text)]
        [Display(Name = "Kod weryfikacyjny")]
        public string ResetCode { get; set; } = string.Empty;


        [Required(ErrorMessage = "Podaj adres email."), EmailAddress(ErrorMessage = "Podaj poprawny adres email.")]
        [RegularExpression(@"^[a-zA-Z0-9._+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Nieprawidłowy adres e-mail. Adres nie może zawierać polskich znaków.")]

        public string Email { get; set; } = string.Empty;
    }
}
