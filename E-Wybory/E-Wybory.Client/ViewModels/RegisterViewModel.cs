using E_Wybory.Client.Validators;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace E_Wybory.Client.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Imię jest obowiązkowe!")]
        public string FirstName { get; set; } = String.Empty;

        [Required(ErrorMessage = "Nazwisko jest obowiązkowe!")]
        public string LastName { get; set; } = String.Empty;

        [Required(ErrorMessage = "PESEL jest obowiązkowy!")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "PESEL musi składać się z dokładnie 11 cyfr.")]
        [RegularExpression("^[0-9]{11}$", ErrorMessage = "PESEL musi zawierać tylko cyfry.")]
        public string PESEL { get; set; } = String.Empty;

        [Required(ErrorMessage = "Data urodzenia jest obowiązkowa!")]
        
        public DateTime DateOfBirth { get; set; } = DateTime.MinValue;

        [RegularExpression(@"^[a-zA-Z0-9._+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Nieprawidłowy adres e-mail. Adres nie może zawierać polskich znaków.")]
        [Required(ErrorMessage = "Adres email jest obowiązkowy!"), EmailAddress]
        public string Email { get; set; } = String.Empty;

        [Required(ErrorMessage = "Numer telefonu jest obowiązkowy!")]
        public string PhoneNumber { get; set; } = String.Empty;

        [ElectionPasswordPolicy]
        [Required(ErrorMessage = "Hasło jest obowiązkowe!")]
        public string Password { get; set; } = String.Empty;

        [Required(ErrorMessage = "Hasło (potwierdzenie) jest obowiązkowe!"), Compare(nameof(Password), ErrorMessage = "Hasła muszą się zgadzać")]
        public string ConfirmPassword { get; set; } = String.Empty;

        [Required(ErrorMessage = "Obwód wyborczy jest obowiązkowy!")]
        [Range(1, int.MaxValue, ErrorMessage = "Proszę wybrać obwód!")]
        public int SelectedDistrictId { get; set; } = 0;

        [ValidateDateOfBirthWithPesel]
        public string DateOfBirthString
        {
            get => DateOfBirth != DateTime.MinValue ? DateOfBirth.ToString("yyyy-MM-dd") : string.Empty;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    //The specified value "DaAa Uro24+2enia" does not conform to the required format, "yyyy-MM-dd".
                    DateOfBirth = DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    //DateOfBirth = new DateTime(2001, 1, 24);
                }
                else
                {
                    DateOfBirth = DateTime.MinValue;
                }
            }
        }
    }
}
