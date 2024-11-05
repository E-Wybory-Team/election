using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface IProvinceManagementService
    {
        Task<List<ProvinceViewModel>> Provinces();
    }
}
