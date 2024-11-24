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

        public async Task<List<VoteViewModel>> GetVotesIdByCandidateId(int candidateId, int electionId)
        {
            var response = await _httpClient.GetFromJsonAsync<List<VoteViewModel>>($"/api/Vote/VotesCandidate/{candidateId}/{electionId}");
            return await Task.FromResult(response);
        }

        public async Task<List<VoteViewModel>> GetVotesByDistrictId(int districtId, int electionId)
        {
            var response = await _httpClient.GetFromJsonAsync<List<VoteViewModel>>($"/api/Vote/VotesDistrict/{districtId}/{electionId}");
            return await Task.FromResult(response);
        }

        public async Task<int> GetVotesNumberByDistrictId(int districtId, int electionId)
        {
            var response = await _httpClient.GetFromJsonAsync<int>($"/api/Vote/VotesNumberDistrict/{districtId}/{electionId}");
            return await Task.FromResult(response);
        }

        public async Task<int> GetValidVotesNumberByDistrictId(int districtId, int electionId)
        {
            var response = await _httpClient.GetFromJsonAsync<int>($"/api/Vote/ValidVotesNumberDistrict/{districtId}/{electionId}");
            return await Task.FromResult(response);
        }

        public async Task<int> GetInvalidVotesNumberByDistrictId(int districtId, int electionId)
        {
            var response = await _httpClient.GetFromJsonAsync<int>($"/api/Vote/InvalidVotesNumberDistrict/{districtId}/{electionId}");
            return await Task.FromResult(response);
        }

        public async Task<int> GetVotesNumberByDistrictCandidate(int districtId, int electionId, int candidateId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<int>($"/api/Vote/VotesNumberDistrictCandidate/{districtId}/{electionId}/{candidateId}");
                return await Task.FromResult(response);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine("Not found votes in this district related to this candidate");
                return 0;
            }

        }

        public async Task<double> GetFrequencyByDistrictIdToHour(int districtId, int electionId, int hourMax)
        {
            var response = await _httpClient.GetFromJsonAsync<double>($"/api/Vote/frequency/{districtId}/{electionId}/{hourMax}");
            return await Task.FromResult(response);
        }
    }
}
