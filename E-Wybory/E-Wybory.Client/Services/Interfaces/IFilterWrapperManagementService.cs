using E_Wybory.Client.FilterData;
using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface IFilterWrapperManagementService
    {
        Task<FilterListWrapper> GetFilteredLists(int? voivodeshipId, int? countyId, int? provinceId);
        Task<List<CandidateViewModel>> GetFilteredCandidates(int? voivodeshipId, int? countyId, int? provinceId, int? districtId);
    }
}
