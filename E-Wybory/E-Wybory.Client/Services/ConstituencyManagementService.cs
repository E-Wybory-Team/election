using E_Wybory.Client.ViewModels;
using System.Net.Http.Json;
using System.Threading.Tasks;
using static E_Wybory.Client.Components.Pages.ConstituencyDelete;
using static E_Wybory.Client.Components.Pages.DetailedStats;

namespace E_Wybory.Client.Services
{
    public class ConstituencyManagementService(IHttpClientFactory clientFactory) : IConstituencyManagementService
    {
            private HttpClient _httpClient = clientFactory.CreateClient("ElectionHttpClient");

            public async Task<List<ConstituencyViewModel>> Constituences()
            {

                var response = await _httpClient.GetFromJsonAsync<List<ConstituencyViewModel>>("/api/Constituency");

                return await Task.FromResult(response);
            }

            public async Task<bool> AddConstituency(ConstituencyViewModel constituencyModel)
            {
                var response = await _httpClient.PostAsJsonAsync("/api/Constituency", constituencyModel);

                return await Task.FromResult(response.IsSuccessStatusCode);
            }

            public async Task<ConstituencyViewModel> GetConstituencyById(int id)
            {
                var response = await _httpClient.GetFromJsonAsync<ConstituencyViewModel>($"/api/Constituency/{id}");
                return await Task.FromResult(response);
            }

        public string? GetConstituencyNameById(int id, List<ConstituencyViewModel> constituences)
        {
            var constituencyName = constituences
                .Where(p => p.idConstituency == id)
                .Select(p => p.constituencyName)
                .FirstOrDefault();

            return constituencyName;
        }

        public async Task<bool> PutConstituency(ConstituencyViewModel constituencyModel)
            {
                var response = await _httpClient.PutAsJsonAsync($"/api/Constituency/{constituencyModel.idConstituency}", constituencyModel);

                return await Task.FromResult(response.IsSuccessStatusCode);
            }

            public async Task<bool> DeleteConstituency(int constituencyId)
            {
                var response = await _httpClient.DeleteAsync($"/api/Constituency/{constituencyId}");

                return await Task.FromResult(response.IsSuccessStatusCode);
            }
            
            public async Task<bool> ConstituencyExists(int constituencyId)
            {
                var response = await _httpClient.GetAsync($"/api/Constituency/exist/{constituencyId}");
                return await Task.FromResult(response.IsSuccessStatusCode);
            }

            public async Task<List<CountyViewModel>> GetCountiesOfConstituency(int constituencyId)
            {
            var response = await _httpClient.GetFromJsonAsync<List<CountyViewModel>>($"/api/Constituency/counties/{constituencyId}");

            return await Task.FromResult(response);
            }
    }
}
