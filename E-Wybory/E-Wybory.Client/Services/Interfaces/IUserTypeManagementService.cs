using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface IUserTypeManagementService
    {
        Task<List<UserTypeViewModel>> UserTypes();
        Task<UserTypeViewModel> GetUserTypeById(int id);
        Task<string> GetUserTypeNameById(int id);
        Task<List<UserTypeViewModel>> GetUserTypesOfGroup(int groupId);
    }
}
