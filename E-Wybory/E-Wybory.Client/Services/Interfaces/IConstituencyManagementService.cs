using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface IConstituencyManagementService
    {
        Task<List<ConstituencyViewModel>> Constituences();
        Task<bool> AddConstituency(ConstituencyViewModel constituencyModel);
        Task<bool> ConstituencyExists(int constituencyId);
        Task<ConstituencyViewModel> GetConstituencyById(int id);
        Task<bool> DeleteConstituency(int constituencyId);
        Task<bool> PutConstituency(ConstituencyViewModel constituencyModel);
        Task<List<CountyViewModel>> GetCountiesOfConstituency(int constituencyId);
        string? GetConstituencyNameById(int id, List<ConstituencyViewModel> constituences);
    }
}
