using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class UserTypeViewModel
    {
        [Required]  
        public int IdUserType { get; set; } = 0;
        
        [Required]
        public string UserTypeName { get; set; } = string.Empty;

        public string UserTypeInfo { get; set; } = null;

        [Required]
        public int IdUserTypesGroup { get; set; } = 0;
    }
}
