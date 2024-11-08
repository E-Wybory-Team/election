using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface ICandidateManagementService
    {
        Task<List<CandidateViewModel>> Candidates();
        Task<bool> AddCandidate(CandidateViewModel candidate);
    }
}
