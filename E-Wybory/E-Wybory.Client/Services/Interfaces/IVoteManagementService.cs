using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface IVoteManagementService
    {
        Task<List<VoteViewModel>> Votes();
        Task<bool> AddVote(VoteViewModel Vote);
        Task<VoteViewModel> GetVoteById(int id);
        Task<bool> DeleteVote(int VoteId);
        Task<List<VoteViewModel>> GetVotesIdByCandidateId(int candidateId, int electionId);
        Task<List<VoteViewModel>> GetVotesByDistrictId(int districtId, int electionId);
        Task<int> GetValidVotesNumberByDistrictId(int districtId, int electionId);
        Task<int> GetInvalidVotesNumberByDistrictId(int districtId, int electionId);
        Task<int> GetVotesNumberByDistrictId(int districtId, int electionId);
        Task<double> GetFrequencyByDistrictIdToHour(int districtId, int electionId, int hourMax);
        Task<int> GetVotesNumberByDistrictCandidate(int districtId, int electionId, int candidateId);
    }
}
