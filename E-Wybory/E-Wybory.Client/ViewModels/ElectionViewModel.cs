using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class ElectionViewModel
    {
        [Required] public int IdElection { get; set; } = 0;
        [Required] public DateTime ElectionStartDate { get; set; }
        [Required] public DateTime ElectionEndDate { get; set; }
        public int ElectionTour { get; set; } = 1;
        [Required] public int IdElectionType { get; set; } = 0;
    }
}
