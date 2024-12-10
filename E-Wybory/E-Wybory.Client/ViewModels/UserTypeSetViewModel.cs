using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class UserTypeSetViewModel
    {
        [Required]
        public int IdUserTypeSet { get; set; } = 0;

        [Required]
        public int IdElectionUser { get; set; } = 0;

        [Required]  
        public int IdUserType { get; set; } = 0;
        
    }
}
