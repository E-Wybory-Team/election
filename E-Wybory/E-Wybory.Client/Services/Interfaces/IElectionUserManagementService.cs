using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface IElectionUserManagementService
    {
        Task<ElectionUserViewModel> GetElectionUserByPersonId(int id);
        Task<bool> UserExists(int id);
        Task<ElectionUserViewModel> GetElectionUserById(int id);
        Task<bool> UserPersonIdExists(int personId);
        Task<bool> PutUser(ElectionUserViewModel userModel);
        Task<bool> PutUserDistrict(ElectionUserViewModel userModel, int districtId);
    }
}
