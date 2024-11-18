using E_Wybory.Client.ViewModels;
using E_Wybory.Domain.Entities;
using System.Net.Http.Json;

namespace E_Wybory.Client.Services
{
    public class VoterManagementService(IHttpClientFactory clientFactory): IVoterManagementService
    {
        private HttpClient _httpClient = clientFactory.CreateClient("ElectionHttpClient");

        public async Task<List<VoterViewModel>> Voters()
        {
            //Properly validate model before that 
            //All properties must be innitialize
            //register.idParty = 1;
            var response = await _httpClient.GetFromJsonAsync<List<VoterViewModel>>("/api/Voter");

            return await Task.FromResult(response);
        }

        public async Task<VoterViewModel> GetVoterById(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<VoterViewModel>($"/api/Voter/{id}");
            return await Task.FromResult(response);
        }

        public async Task<bool> AddVoter(VoterViewModel Voter)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/Voter", Voter);

            return await Task.FromResult(response.IsSuccessStatusCode);
        }

        public async Task<bool> PutVoter(VoterViewModel Voter)
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/Voter/{Voter.IdVoter}", Voter);

            return await Task.FromResult(response.IsSuccessStatusCode);
        }


        public async Task<bool> DeleteVoter(int VoterId)
        {
            var response = await _httpClient.DeleteAsync($"/api/Voter/{VoterId}");
            return await Task.FromResult(response.IsSuccessStatusCode);
        }

        public async Task<bool> VoterExists(int voterId)
        {
            var response = await _httpClient.GetAsync($"/api/Voter/exist/{voterId}");
            return await Task.FromResult(response.IsSuccessStatusCode);
        }

        public async Task<int> GetVoterIdByElectionUserId(int userId)
        {
            var response = await _httpClient.GetFromJsonAsync<int>($"/api/Voter/voter/{userId}");
            return await Task.FromResult(response);
        }

        public async Task<VoterViewModel> GetVoterByElectionUserId(int userId)
        {
            var response = await _httpClient.GetFromJsonAsync<VoterViewModel>($"/api/Voter/voterRecord/{userId}");
            return await Task.FromResult(response);
        }
    }
}
