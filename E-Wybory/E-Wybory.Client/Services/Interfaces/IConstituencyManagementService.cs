using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface IConstituencyManagementService
    {
        Task<List<ConstituencyViewModel>> Constituences();
        Task<bool> AddConstituency(ConstituencyViewModel constituencyModel);
    }
}
