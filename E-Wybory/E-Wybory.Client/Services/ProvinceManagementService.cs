using E_Wybory.Client.ViewModels;
using E_Wybory.Domain.Entities;
using System.Net.Http.Json;

namespace E_Wybory.Client.Services
{
    public class ProvinceManagementService(IHttpClientFactory clientFactory) : IProvinceManagementService
    {
        private HttpClient _httpClient = clientFactory.CreateClient("ElectionHttpClient");

        public async Task<List<ProvinceViewModel>> Provinces()
        {
            //Properly validate model before that 
            //All properties must be innitialize
            //register.idParty = 1;
            var response = await _httpClient.GetFromJsonAsync<List<ProvinceViewModel>>("/api/Province");

            return await Task.FromResult(response);
        }

        public async Task<ProvinceViewModel> GetProvinceById(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<ProvinceViewModel>($"/api/Province/{id}");
            return await Task.FromResult(response);
        }

        public int? GetCountyIdFromProvince(int provinceId, List<ProvinceViewModel> provinces)
        {
            var countyId = provinces
                .Where(d => d.IdProvince == provinceId)
                .Select(d => d.IdCounty)
                .FirstOrDefault();

            return countyId;
        }

    }
}
