using E_Wybory.Client.ViewModels;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace E_Wybory.Client.Services
{
    public class VoivodeshipManagementService(IHttpClientFactory clientFactory) : IVoivodeshipManagementService
    {
            private HttpClient _httpClient = clientFactory.CreateClient("ElectionHttpClient");

            public async Task<List<VoivodeshipViewModel>> Voivodeships()
            {
                //Properly validate model before that 
                //All properties must be innitialize
                var response = await _httpClient.GetFromJsonAsync<List<VoivodeshipViewModel>>("/api/Voivodeship");

                return await Task.FromResult(response);
            }

            public async Task<VoivodeshipViewModel> GetVoivodeshipById(int id)
            {
                var response = await _httpClient.GetFromJsonAsync<VoivodeshipViewModel>($"/api/Voivodeship/{id}");
                return await Task.FromResult(response);
            }
    }
}
