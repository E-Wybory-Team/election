using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface IProvinceManagementService
    {
        Task<List<ProvinceViewModel>> Provinces();
        Task<ProvinceViewModel> GetProvinceById(int id);
        int? GetCountyIdFromProvince(int provinceId, List<ProvinceViewModel> provinces);
    }
}
