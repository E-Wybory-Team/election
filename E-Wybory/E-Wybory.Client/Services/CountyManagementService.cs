using E_Wybory.Client.ViewModels;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Net.Http.Json;

namespace E_Wybory.Client.Services
{
    public class CountyManagementService(IHttpClientFactory clientFactory) : ICountyManagementService
    {
        private HttpClient _httpClient = clientFactory.CreateClient("ElectionHttpClient");

        public async Task<List<CountyViewModel>> Counties()
        {
            //Properly validate model before that 
            //All properties must be innitialize
            var response = await _httpClient.GetFromJsonAsync<List<CountyViewModel>>("/api/County");

            return await Task.FromResult(response);
        }

        public async Task<CountyViewModel> GetCountyById(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<CountyViewModel>($"/api/County/{id}");
            return await Task.FromResult(response);
        }

        public int? GetVoivodeshipIdFromCounty(int idCounty, List<CountyViewModel> counties)
        {
            return counties
                    .Where(v => v.IdCounty == idCounty)
                    .Select(v => v.IdVoivodeship)
                    .FirstOrDefault();
        }
    }
}
