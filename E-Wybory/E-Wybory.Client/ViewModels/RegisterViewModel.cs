using System.ComponentModel.DataAnnotations;

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
                    DateOfBirth = DateTime.Parse(value);
                }
            }
        }
    }
}
