using E_Wybory.Client.ViewModels;
using System.Net.Http.Json;

namespace E_Wybory.Client.Services
{
    public class ElectionVoterManagementService(IHttpClientFactory clientFactory): IElectionVoterManagementService
    {
        private HttpClient _httpClient = clientFactory.CreateClient("ElectionHttpClient");

        public async Task<ElectionVoterViewModel> GetElectionVoter(int idVoter, int idElection)
        {
            var response = await _httpClient.GetFromJsonAsync<ElectionVoterViewModel>($"/api/ElectionVoter/{idVoter}/{idElection}");

            return await Task.FromResult(response);
        }

        public async Task<bool> AddElectionVoter(ElectionVoterViewModel Voter)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/ElectionVoter", Voter);

            return await Task.FromResult(response.IsSuccessStatusCode);
        }

        public async Task<bool> ElectionVoterExists(int voterId, int electionId)
        {
            var response = await _httpClient.GetAsync($"/api/ElectionVoter/exist/{voterId}/{electionId}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<bool>();
                return result;
            }
            else
            {
                return false;
            }
        }


    }
}
