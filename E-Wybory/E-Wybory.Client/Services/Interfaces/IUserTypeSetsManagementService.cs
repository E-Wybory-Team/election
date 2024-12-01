using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface IUserTypeSetsManagementService
    {
        Task<List<UserTypeSetViewModel>> UserTypesSets();
        Task<List<CommissionerViewModel>> CommissionersOfDistrict(int districtId);
        Task<bool> AddUserTypeSet(UserTypeSetViewModel userTypeSetViewModel);
        Task<bool> DeleteUserTypeSet(int userTypeSetId);
        Task<bool> UserWithTypeGroupExists(int userTypeId, int electionUserId);
    }
}
