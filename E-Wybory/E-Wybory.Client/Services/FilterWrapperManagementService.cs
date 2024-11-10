﻿using E_Wybory.Client.FilterData;
using E_Wybory.Client.ViewModels;
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

        public async Task<FilterListWrapperFull> GetFilteredLists(int? voivodeshipId, int? countyId, int? provinceId)
        {
            var url = "/api/FilterWrapper/Lists";
            var queryParameters = new List<string>();

            if (voivodeshipId.HasValue)
            {
                queryParameters.Add($"voivodeshipId={voivodeshipId.Value}");
            }
            if (countyId.HasValue)
            {
                queryParameters.Add($"countyId={countyId.Value}");
            }
            if (provinceId.HasValue)
            {
                queryParameters.Add($"provinceId={provinceId.Value}");
            }

            if (queryParameters.Any())
            {
                url += "?" + string.Join("&", queryParameters);
            }


            var response = await _httpClient.GetFromJsonAsync<FilterListWrapperFull>(url);
            return response;
        }

        public async Task<List<CandidatePersonViewModel>> GetFilteredCandidates(int? electionTypeId, int? voivodeshipId, int? countyId, int? provinceId, int? districtId)
        {
            var url = "/api/FilterWrapper/Candidates";
            var queryParameters = new List<string>();

            if (electionTypeId.HasValue)
            {
                queryParameters.Add($"electionTypeId={electionTypeId.Value}");
            }
            if (voivodeshipId.HasValue)
            {
                queryParameters.Add($"voivodeshipId={voivodeshipId.Value}");
            }
            if (countyId.HasValue)
            {
                queryParameters.Add($"countyId={countyId.Value}");
            }
            if (provinceId.HasValue)
            {
                queryParameters.Add($"provinceId={provinceId.Value}");
            }
            if (districtId.HasValue)
            {
                queryParameters.Add($"districtId={districtId.Value}");
            }

            if (queryParameters.Any())
            {
                url += "?" + string.Join("&", queryParameters);
            }

            var response = await _httpClient.GetFromJsonAsync<List<CandidatePersonViewModel>>(url);
            return response;
        }

    }
}
