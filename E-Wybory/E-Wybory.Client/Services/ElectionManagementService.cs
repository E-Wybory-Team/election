using E_Wybory.Client.ViewModels;
using System.Net.Http.Json;

namespace E_Wybory.Client.Services
{
    public class ElectionManagementService(IHttpClientFactory clientFactory) : IElectionManagementService
    {
        private HttpClient _httpClient = clientFactory.CreateClient("ElectionHttpClient");

        public async Task<List<ElectionViewModel>> Elections()
        {
            //Properly validate model before that 
            //All properties must be innitialize
            //register.idParty = 1;
            var response = await _httpClient.GetFromJsonAsync<List<ElectionViewModel>>("/api/Election");

            return await Task.FromResult(response);
        }

    }
}
