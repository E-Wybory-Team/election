using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class VoterViewModel
    {
        [Required(ErrorMessage ="ID wyborcy jest obowiązkowe!")]
        public int IdVoter { get; set; } = 0;

        [Required(ErrorMessage = "ID użytkownika jest obowiązkowe!")]
        public int IdElectionUser { get; set; } = 0;

        public int? IdDistrict { get; set; } = null;
    }
}
