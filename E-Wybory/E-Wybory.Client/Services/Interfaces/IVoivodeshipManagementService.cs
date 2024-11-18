using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface IVoivodeshipManagementService
    {
        Task<List<VoivodeshipViewModel>> Voivodeships();
        Task<VoivodeshipViewModel> GetVoivodeshipById(int id);
    }
}
