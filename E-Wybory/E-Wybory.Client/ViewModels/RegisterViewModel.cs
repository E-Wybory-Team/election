using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Surname { get; set; }

        [Required]
        public string PESEL { get; set; }

        [Required]
        public DateTime Birthdate { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string ConfirmPassword { get; set; }

        [Required]
        public int idDistrict { get; set; }
    }
}
