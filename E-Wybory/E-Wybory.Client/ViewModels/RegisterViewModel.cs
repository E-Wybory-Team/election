using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class RegisterViewModel
    {
        public string Name { get; set; }
        
        public string Surname { get; set; }

        public string PESEL { get; set; }
        
        public DateTime Birthdate { get; set; }
        
        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public int idDistrict { get; set; }
    }
}
