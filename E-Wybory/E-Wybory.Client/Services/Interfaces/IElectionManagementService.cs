using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface IElectionManagementService
    {
        Task<List<ElectionViewModel>> Elections();
        Task<List<ElectionViewModel>> GetActiveElections();
        int? GetElectionTypeIdFromElection(int electionId, List<ElectionViewModel> elections);
        Task<bool> AddElection(ElectionViewModel election);
        Task<bool> PutElection(ElectionViewModel election);
        Task<bool> DeleteElection(int electionId);
        Task<ElectionViewModel> GetElectionById(int id);
        Task<List<ElectionViewModel>> GetElectionsOfType(int electionTypeId);
        Task<List<ElectionViewModel>> GetNewestElections();
        Task<ElectionViewModel> GetNewestElectionOfElectionType(int electionTypeId);
    }
}
