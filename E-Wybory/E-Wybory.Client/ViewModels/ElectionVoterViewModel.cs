using System.ComponentModel.DataAnnotations;

namespace E_Wybory.Client.ViewModels
{
    public class ElectionVoterViewModel
    {
        [Required] public int IdElectionVoter { get; set; } = 0;
        [Required] public int IdElection { get; set; } = 0;
        [Required] public int IdVoter { get; set; } = 0;
        public DateTime VoteTime { get; set; } = DateTime.MinValue;

    }
}
