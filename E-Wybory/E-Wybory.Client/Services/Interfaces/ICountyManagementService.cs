using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface ICountyManagementService
    {
        Task<List<CountyViewModel>> Counties();
        Task<CountyViewModel> GetCountyById(int id);
        int? GetVoivodeshipIdFromCounty(int idCounty, List<CountyViewModel> counties);
    }
}
