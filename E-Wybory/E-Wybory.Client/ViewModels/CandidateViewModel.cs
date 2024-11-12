using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class CandidateViewModel
    {
        [Required(ErrorMessage = "Wybór kandydata jest obowiązkowy!")] public int IdCandidate { get; set; } = 0;
        public string? CampaignDescription { get; set; } = null;
        [Required(ErrorMessage = "Zawód jest obowiązkowy!")] public string JobType { get; set; } = String.Empty;
        [Required(ErrorMessage = "Miejsce zamieszkania jest obowiązkowe!")] public string PlaceOfResidence { get; set; } = String.Empty;
        [Required(ErrorMessage = "Pozycja na liście jest obowiązkowa!")] public int PositionNumber { get; set; } = 0;
        public string? EducationStatus { get; set; } = null;
        public string? Workplace { get; set; } = null;
        [Required] public int IdPerson { get; set; } = 0;
        [Required(ErrorMessage = "Obwód jest obowiązkowy!")] public int IdDistrict { get; set; } = 0;
        public int? IdParty { get; set; } = null;
        [Required(ErrorMessage = "Wybór wyborów jest obowiązkowy!")] public int IdElection { get; set; } = 0;
    }
}
