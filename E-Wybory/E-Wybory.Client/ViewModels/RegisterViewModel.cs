using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string Name { get; set; } = String.Empty;

        [Required]
        public string Surname { get; set; } = String.Empty;

        [Required]
        public string PESEL { get; set; } = String.Empty;

        [Required]
        public DateTime Birthdate { get; set; } = DateTime.MinValue;

        [Required]
        public string Email { get; set; } = String.Empty;

        [Required]
        public string PhoneNumber { get; set; } = String.Empty;

        [Required]
        public string Password { get; set; } = String.Empty;

        [Required]
        public string ConfirmPassword { get; set; } = String.Empty;

        [Required]
        public int idDistrict { get; set; } = 0;
    }
}
