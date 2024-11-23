using E_Wybory.Client.ViewModels;
using System.Net.Http;
using System.Net.Http.Json;

namespace E_Wybory.Client.Services
{
    public class ElectionUserManagementService(IHttpClientFactory clientFactory): IElectionUserManagementService
    {
        private HttpClient _httpClient = clientFactory.CreateClient("ElectionHttpClient");

        public async Task<ElectionUserViewModel> GetElectionUserByPersonId(int id)
        {
            var response = new ElectionUserViewModel();

            try
            {
                response = await _httpClient.GetFromJsonAsync<ElectionUserViewModel>($"/api/ElectionUser/person/{id}");
            }
            catch(Exception ex)
            {
                
            }
            return await Task.FromResult(response);
        }

        public async Task<ElectionUserViewModel> GetElectionUserById(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<ElectionUserViewModel>($"/api/ElectionUser/{id}");
            return await Task.FromResult(response);
        }

        public async Task<bool> UserExists(int id)
        {
            var response = await _httpClient.GetAsync($"/api/ElectionUser/exist/{id}");
            return await Task.FromResult(response.IsSuccessStatusCode);
        }

        public async Task<bool> UserPersonIdExists(int personId)
        {
            var response = await _httpClient.GetAsync($"/api/ElectionUser/existPerson/{personId}");
            return await Task.FromResult(response.IsSuccessStatusCode);
        }


    }
}
