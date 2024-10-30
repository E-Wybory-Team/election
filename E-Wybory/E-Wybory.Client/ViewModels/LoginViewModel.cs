using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Nazwa użytkownika jest wymagana.")]
        public string Username { get; set; } = String.Empty;

        [Required(ErrorMessage = "Hasło jest wymagane.")]
        public string Password { get; set; } = String.Empty;
    }
}
