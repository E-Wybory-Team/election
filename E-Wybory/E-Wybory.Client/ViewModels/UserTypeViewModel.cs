using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class UserTypeViewModel
    {
        [Required]  
        public int IdUserType { get; set; } = 0;
        
        [Required]
        public string UserTypeName { get; set; } = string.Empty;
    }
}
