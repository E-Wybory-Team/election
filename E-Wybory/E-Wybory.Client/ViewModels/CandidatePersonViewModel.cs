using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class CandidatePersonViewModel
    {
        [Required]
        public CandidateViewModel candidateViewModel { get; set; } = new CandidateViewModel();

        [Required]
        public PersonViewModel personViewModel { get; set; } = new PersonViewModel();
    }
}
