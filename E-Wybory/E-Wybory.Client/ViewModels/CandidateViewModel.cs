using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class CandidateViewModel
    {
        [Required] public int IdCandidate { get; set; } = 0;
        public string? CampaignDescription { get; set; } = null;
        [Required] public string JobType { get; set; } = String.Empty;
        [Required] public string PlaceOfResidence { get; set; } = String.Empty;
        [Required] public int PositionNumber { get; set; } = 0;
        public string? EducationStatus { get; set; } = null;
        public string? Workplace { get; set; } = null;
        [Required] public int IdPerson { get; set; } = 0;
        [Required] public int IdDistrict { get; set; } = 0;
        public int? IdParty { get; set; } = null;
        [Required] public int IdElection { get; set; } = 0;
    }
}
