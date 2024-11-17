using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface IElectionUserManagementService
    {
        Task<ElectionUserViewModel> GetElectionUserByPersonId(int id);
        Task<bool> UserExists(int id);
    }
}
