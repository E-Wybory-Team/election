using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        public string Username { get; set; } = String.Empty;

        [Required]
        public string Password { get; set; } = String.Empty;
    }
}
