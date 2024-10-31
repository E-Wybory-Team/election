using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace E_Wybory.Client.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string FirstName { get; set; } = String.Empty;

        [Required]
        public string LastName { get; set; } = String.Empty;

        [Required]
        public string PESEL { get; set; } = String.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; } = DateTime.MinValue;

        [Required, EmailAddress]
        public string Email { get; set; } = String.Empty;

        [Required]
        public string PhoneNumber { get; set; } = String.Empty;

        [Required]
        public string Password { get; set; } = String.Empty;

        [Required, Compare(nameof(Password), ErrorMessage = "Hasła muszą się zgadzać")]
        public string ConfirmPassword { get; set; } = String.Empty;

        [Required]
        public int SelectedDistrictId { get; set; } = 0;

        public string DateOfBirthString
        {
            get => DateOfBirth != DateTime.MinValue ? DateOfBirth.ToString("Data Urodzenia") : string.Empty;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    //The specified value "DaAa Uro24+2enia" does not conform to the required format, "yyyy-MM-dd".
                    //DateOfBirth = DateTime.ParseExact(value, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                    DateOfBirth = new DateTime(2001, 1, 24);
                }
            }
        }
    }
}
