using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class ElectionUserViewModel
    {
        [Required(ErrorMessage = "Id użytkownika jest obowiązkowe!")]
        public int IdElectionUser { get; set; }

        [Required(ErrorMessage = "Adres E-mail użytkownika jest obowiązkowy!")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Nr tel. użytkownika jest obowiązkowy!")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Hasło użytkownika jest obowiązkowe!")]
        public string Password { get; set; } = null!;

        [Required]
        public int IdPerson { get; set; }

        [Required(ErrorMessage = "Obwód, do którego jest przypisany użytkownik jest obowiązkowy!")]
        public int IdDistrict { get; set; }

        public string? UserSecret { get; set; }
    }
}
