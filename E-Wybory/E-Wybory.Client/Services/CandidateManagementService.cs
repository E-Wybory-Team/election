using E_Wybory.Client.ViewModels;
using E_Wybory.Domain.Entities;
using System.Net.Http.Json;

namespace E_Wybory.Client.Services
{
    public class CandidateManagementService(IHttpClientFactory clientFactory) : ICandidateManagementService
    {
        private HttpClient _httpClient = clientFactory.CreateClient("ElectionHttpClient");

        public async Task<List<CandidateViewModel>> Candidates()
        {
            //Properly validate model before that 
            //All properties must be innitialize
            //register.idParty = 1;
            var response = await _httpClient.GetFromJsonAsync<List<CandidateViewModel>>("/api/Candidate");

            return await Task.FromResult(response);
        }

        public async Task<bool> AddCandidate(CandidateViewModel candidate)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/Candidate", candidate);

            return await Task.FromResult(response.IsSuccessStatusCode);
        }

    }
}
