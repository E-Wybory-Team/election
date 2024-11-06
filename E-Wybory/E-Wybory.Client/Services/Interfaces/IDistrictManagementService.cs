using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface IDistrictManagementService
    {
        Task<List<DistrictViewModel>> Districts();
        int? GetProvinceIdFromDistrict(int idDistrict, List<DistrictViewModel> districts);
    }
}
