using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class UserInfoViewModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Surname { get; set; } = string.Empty;
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public UserTypeViewModel CurrentUserType { get; set; } = null!;
        [Required]
        public List<UserTypeViewModel> AvailableUserTypes { get; set; } = new List<UserTypeViewModel>();

    }
}
