using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface ICandidateManagementService
    {
        Task<List<CandidateViewModel>> Candidates();
        Task<bool> AddCandidate(CandidateViewModel candidate);
        Task<bool> PutCandidate(CandidateViewModel candidate);
        Task<bool> CandidateExists(int candidateId);
        Task<CandidateViewModel> GetCandidateById(int id);
        Task<bool> DeleteCandidate(int candidateId);
        Task<List<CandidateViewModel>> GetCandidatesByElectionDistrictId(int electionId, int districtId);
        bool IfCandidateListHasCandidate(int candidateId, List<CandidatePersonViewModel> candidates);
    }
}
