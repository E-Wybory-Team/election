using E_Wybory.Client.ViewModels;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace E_Wybory.Client.Services
{
    public class ConstituencyManagementService(IHttpClientFactory clientFactory) : IConstituencyManagementService
    {
            private HttpClient _httpClient = clientFactory.CreateClient("ElectionHttpClient");

            public async Task<List<ConstituencyViewModel>> Constituences()
            {
                //Properly validate model before that 
                //All properties must be innitialize
                var response = await _httpClient.GetFromJsonAsync<List<ConstituencyViewModel>>("/api/Constituency");

                return await Task.FromResult(response);
            }

            public async Task<bool> AddConstituency(ConstituencyViewModel constituencyModel)
            {
                var response = await _httpClient.PostAsJsonAsync("/api/Constituency", constituencyModel);

                return await Task.FromResult(response.IsSuccessStatusCode);
            }
    }
}
