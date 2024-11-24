using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class VoteViewModel
    {
        [Required(ErrorMessage ="ID głosu jest obowiązkowe!")]
        public int IdVote { get; set; } = 0;

        [Required(ErrorMessage = "Ważność głosu jest obowiązkowa!")]
        public bool IsValid { get; set; } = false;

        [Required(ErrorMessage = "Wybory są obowiązkowe!")]
        public int IdElection { get; set; } = 0;

        [Required(ErrorMessage = "ID kandydata jest obowiązkowe!")]
        public int IdCandidate { get; set; } = 0;

        [Required(ErrorMessage = "ID obwodu jest obowiązkowe!")]
        public int IdDistrict { get; set; } = 0;
    }
}
