using E_Wybory.Client.ViewModels;
using E_Wybory.Domain.Entities;
using System.Net.Http.Json;

namespace E_Wybory.Client.Services
{
    public class VoteManagementService(IHttpClientFactory clientFactory): IVoteManagementService
    {
        private HttpClient _httpClient = clientFactory.CreateClient("ElectionHttpClient");

        public async Task<List<VoteViewModel>> Votes()
        {
            //Properly validate model before that 
            //All properties must be innitialize
            //register.idParty = 1;
            var response = await _httpClient.GetFromJsonAsync<List<VoteViewModel>>("/api/Vote");

            return await Task.FromResult(response);
        }

        public async Task<VoteViewModel> GetVoteById(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<VoteViewModel>($"/api/Vote/{id}");
            return await Task.FromResult(response);
        }

        public async Task<bool> AddVote(VoteViewModel Vote)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/Vote", Vote);

            return await Task.FromResult(response.IsSuccessStatusCode);
        }

        public async Task<bool> DeleteVote(int VoteId)
        {
            var response = await _httpClient.DeleteAsync($"/api/Vote/{VoteId}");
            return await Task.FromResult(response.IsSuccessStatusCode);
        }

        public async Task<List<VoteViewModel>> GetVotesIdByCandidateId(int candidateId)
        {
            var response = await _httpClient.GetFromJsonAsync<List<VoteViewModel>>($"/api/Vote/VotesCandidate/{candidateId}");
            return await Task.FromResult(response);
        }

        public async Task<List<VoteViewModel>> GetVotesByDistrictId(int districtId)
        {
            var response = await _httpClient.GetFromJsonAsync<List<VoteViewModel>>($"/api/Vote/VotesDistrict/{districtId}");
            return await Task.FromResult(response);
        }
    }
}
