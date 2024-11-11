using E_Wybory.Client.ViewModels;
using System.Net.Http;
using System.Net.Http.Json;

namespace E_Wybory.Client.Services
{
    public class DistrictManagementService(IHttpClientFactory clientFactory) : IDistrictManagementService
    {
        private HttpClient _httpClient = clientFactory.CreateClient("ElectionHttpClient");

        public async Task<List<DistrictViewModel>> Districts()
        {
            //Properly validate model before that 
            //All properties must be innitialize
            //register.idDistrict = 1;
            var response = await _httpClient.GetFromJsonAsync<List<DistrictViewModel>>("/api/District");

            return await Task.FromResult(response);
        }

        public async Task<bool> AddDistrict(DistrictViewModel district)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/District", district);

            return await Task.FromResult(response.IsSuccessStatusCode);
        }

        public async Task<bool> PutDistrict(DistrictViewModel district)
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/District/{district.IdDistrict}", district);

            return await Task.FromResult(response.IsSuccessStatusCode);
        }

        public int? GetProvinceIdFromDistrict(int idDistrict, List<DistrictViewModel> districts)
        {
            var provinceId = districts
                .Where(d => d.IdDistrict == idDistrict)
                .Select(d => d.IdProvince)
                .FirstOrDefault();

            return provinceId;
        }

    }
}
