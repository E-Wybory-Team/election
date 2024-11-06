using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface IProvinceManagementService
    {
        Task<List<ProvinceViewModel>> Provinces();
        int? GetCountyIdFromProvince(int provinceId, List<ProvinceViewModel> provinces);
    }
}
