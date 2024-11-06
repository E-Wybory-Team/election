using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface IElectionManagementService
    {
        Task<List<ElectionViewModel>> Elections();
        int? GetElectionTypeIdFromElection(int electionId, List<ElectionViewModel> elections);
    }
}
