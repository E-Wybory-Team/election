using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface IElectionTypeManagementService
    {
        Task<List<ElectionTypeViewModel>> ElectionTypes();
    }
}
