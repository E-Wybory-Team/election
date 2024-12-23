﻿using E_Wybory.Client.Components.Pages;
using E_Wybory.Client.FilterData;
using E_Wybory.Client.ViewModels;
using E_Wybory.Domain.Entities;
using System.Net.Http.Json;

namespace E_Wybory.Client.Services
{
    public class FilterWrapperManagementService : IFilterWrapperManagementService
    {
        private readonly HttpClient _httpClient;

        public FilterWrapperManagementService(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient("ElectionHttpClient");
        }

        private string BuildUrlWithParameters(string baseUrl, Dictionary<string, int?> parameters)
        {
            var queryParameters = new List<string>();
            foreach (var param in parameters)
            {
                if (param.Value.HasValue)
                {
                    queryParameters.Add($"{param.Key}={param.Value.Value}");
                }
            }

            if (queryParameters.Any())
            {
                baseUrl += "?" + string.Join("&", queryParameters);
            }

            return baseUrl;
        }

        public async Task<FilterListWrapperFull> GetFilteredLists(int? voivodeshipId, int? countyId, int? provinceId)
        {
            var url = BuildUrlWithParameters("/api/FilterWrapper/Lists", new Dictionary<string, int?>
            {
                { "voivodeshipId", voivodeshipId },
                { "countyId", countyId },
                { "provinceId", provinceId }
            });

            var response = await _httpClient.GetFromJsonAsync<FilterListWrapperFull>(url);
            return response;
        }

        public async Task<FilterListWrapper> GetFilteredListsWrapper(int? voivodeshipId, int? countyId, int? provinceId)
        {
            var url = BuildUrlWithParameters("/api/FilterWrapper/ListsWrapper", new Dictionary<string, int?>
            {
                { "voivodeshipId", voivodeshipId },
                { "countyId", countyId },
                { "provinceId", provinceId }
            });

            var response = await _httpClient.GetFromJsonAsync<FilterListWrapper>(url);
            return response;
        }

        public async Task<FilterListWrapper> GetFilteredLists(int? voivodeshipId, int? countyId)
        {
            var url = BuildUrlWithParameters("/api/FilterWrapper/RegionLists", new Dictionary<string, int?>
            {
            { "voivodeshipId", voivodeshipId },
            { "countyId", countyId }
            });

            var response = await _httpClient.GetFromJsonAsync<FilterListWrapper>(url);
            return response;
        }

        public async Task<FilterListWrapperDistrict> GetFilteredListsDistricts(int? constituencyId, int? voivodeshipId, int? countyId)
        {
            var url = BuildUrlWithParameters("/api/FilterWrapper/DistrictLists", new Dictionary<string, int?>
            {
            {"constituencyId", constituencyId},
            { "voivodeshipId", voivodeshipId },
            { "countyId", countyId }
            });

            var response = await _httpClient.GetFromJsonAsync<FilterListWrapperDistrict>(url);
            return response;
        }

        public async Task<List<CandidatePersonViewModel>> GetFilteredCandidates(
            int? electionTypeId, int? voivodeshipId, int? countyId, int? provinceId, int? districtId)
            {
            var url = BuildUrlWithParameters("/api/FilterWrapper/Candidates", new Dictionary<string, int?>
            { 
                { "electionTypeId", electionTypeId },
                { "voivodeshipId", voivodeshipId },
                { "countyId", countyId },
                { "provinceId", provinceId },
                { "districtId", districtId }
            });

            var response = await _httpClient.GetFromJsonAsync<List<CandidatePersonViewModel>>(url);
            return response;
            }



        public async Task<List<CandidatePersonViewModel>> GetFilteredCandidatesFromElection(int? electionId,  int? districtId)
        {
            var url = BuildUrlWithParameters("/api/FilterWrapper/CandidatesElection", new Dictionary<string, int?>
            {
                { "electionId", electionId },
                { "districtId", districtId }
            });
            var response = await _httpClient.GetFromJsonAsync<List<CandidatePersonViewModel>>(url);
            return response;
        }


        public async Task<List<CandidatePersonViewModel>> GetFilteredCandidatesFromElectionRegions(int? electionId,int? voivodeshipId,int? countyId,int? provinceId,int? districtId)
        {
            var url = BuildUrlWithParameters("/api/FilterWrapper/CandidatesElectionRegions", new Dictionary<string, int?>
            {
                { "electionId", electionId },
                { "voivodeshipId", voivodeshipId },
                { "countyId", countyId },
                { "provinceId", provinceId },
                { "districtId", districtId }
            });
            var response = await _httpClient.GetFromJsonAsync<List<CandidatePersonViewModel>>(url);
            return response;
        }


        public async Task<List<DistrictViewModel>> GetFilteredDistricts(
            int? constituencyId, int? voivodeshipId, int? countyId, int? provinceId)
        {
            var url = BuildUrlWithParameters("/api/FilterWrapper/Districts", new Dictionary<string, int?>
            {
                {"constituencyId", constituencyId},
                { "voivodeshipId", voivodeshipId },
                { "countyId", countyId },
                { "provinceId", provinceId }
            });

            var response = await _httpClient.GetFromJsonAsync<List<DistrictViewModel>>(url);
            return response;
        }

        public async Task<List<UserPersonViewModel>> GetFilteredUsers(
            int? voivodeshipId, int? countyId, int? provinceId, int? districtId)
        {
            var url = BuildUrlWithParameters("/api/FilterWrapper/Users", new Dictionary<string, int?>
            {
                { "voivodeshipId", voivodeshipId },
                { "countyId", countyId },
                { "provinceId", provinceId },
                { "districtId", districtId }
            });

            var response = await _httpClient.GetFromJsonAsync<List<UserPersonViewModel>>(url);
            return response;
        }
        

        public async Task<List<CandidateViewModel>> GetFilteredCandidatesFromParty(
            int? partyId, int? electionId)
        {
            var url = BuildUrlWithParameters("/api/FilterWrapper/PartiesCandidates", new Dictionary<string, int?>
            {
                { "partyId", partyId },
                { "electionIdId", electionId }
            });

            var response = await _httpClient.GetFromJsonAsync<List<CandidateViewModel>>(url);
            return response;
        }


        public async Task<List<CandidatePersonViewModel>> GetFilteredCandidatesWithoutDistrict(int electionId)
        {
            var response = await _httpClient.GetFromJsonAsync<List<CandidatePersonViewModel>>($"api/FilterWrapper/CandidatesWithoutRegions/{electionId}");
            return response;
        }


        public async Task<List<string>> GetRegionsOfDistrict(int districtId)
        {
            var response = await _httpClient.GetFromJsonAsync<List<string>>($"api/FilterWrapper/RegionsOfDistrict/{districtId}");
            return response;
        }
    }
}
