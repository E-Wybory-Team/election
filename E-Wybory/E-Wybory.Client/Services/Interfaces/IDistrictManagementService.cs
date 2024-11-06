using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface IDistrictManagementService
    {
        Task<List<DistrictViewModel>> Districts();
    }
}
