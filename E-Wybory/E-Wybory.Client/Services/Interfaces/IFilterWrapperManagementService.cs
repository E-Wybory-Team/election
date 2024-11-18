﻿using E_Wybory.Client.FilterData;
using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public interface IFilterWrapperManagementService
    {
        Task<FilterListWrapperFull> GetFilteredLists(int? voivodeshipId, int? countyId, int? provinceId);
        Task<FilterListWrapper> GetFilteredLists(int? voivodeshipId, int? countyId);
        Task<List<CandidatePersonViewModel>> GetFilteredCandidates(int? electionTypeId, int? voivodeshipId, int? countyId, int? provinceId, int? districtId);
        Task<List<DistrictViewModel>> GetFilteredDistricts(int? constituencyId, int? voivodeshipId, int? countyId, int? provinceId);
        Task<FilterListWrapperDistrict> GetFilteredListsDistricts(int? constituencyId, int? voivodeshipId, int? countyId);
        Task<List<UserPersonViewModel>> GetFilteredUsers(
            int? voivodeshipId, int? countyId, int? provinceId, int? districtId);

        Task<FilterListWrapper> GetFilteredListsWrapper(int? voivodeshipId, int? countyId, int? provinceId);
    }
}