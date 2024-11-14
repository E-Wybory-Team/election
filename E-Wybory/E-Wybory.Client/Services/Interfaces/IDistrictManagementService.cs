using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface IDistrictManagementService
    {
        Task<List<DistrictViewModel>> Districts();
        int? GetProvinceIdFromDistrict(int idDistrict, List<DistrictViewModel> districts);
        Task<DistrictViewModel> GetDistrictById(int id);
        Task<bool> AddDistrict(DistrictViewModel district);
        Task<bool> PutDistrict(DistrictViewModel district);
        Task<bool> DistrictExists(int districtId);
        Task<bool> DeleteDistrict(int districtId);

    }
}
