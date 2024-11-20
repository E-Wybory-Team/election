using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface IElectionVoterManagementService
    {
        Task<bool> AddElectionVoter(ElectionVoterViewModel Voter);
        Task<ElectionVoterViewModel> GetElectionVoter(int idVoter, int idElection);
        Task<bool> ElectionVoterExists(int voterId, int electionId);
    }
}
