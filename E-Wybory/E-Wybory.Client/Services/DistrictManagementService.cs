using E_Wybory.Client.ViewModels;
using System.Net.Http;
using System.Net.Http.Json;

namespace E_Wybory.Client.Services
{
    public class DistrictManagementService(HttpClient httpClient) : IDistrictManagementService
    {
        private HttpClient _httpClient = httpClient;

        public async Task<List<DistrictViewModel>> Districts()
        {
            //Properly validate model before that 
            //All properties must be innitialize
            //register.idDistrict = 1;
            var response = await _httpClient.GetFromJsonAsync<List<DistrictViewModel>>("/api/District");

            return await Task.FromResult(response);
        }

    }
}
