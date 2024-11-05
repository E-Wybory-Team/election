using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class ElectionViewModel
    {
        [Required] public int IdElection { get; set; } = 0;
        [Required] public DateTime ElectionStart { get; set; }
        [Required] public DateTime ElectionEnd { get; set; }
        public int ElectionTour { get; set; } = 1;
    }
}
