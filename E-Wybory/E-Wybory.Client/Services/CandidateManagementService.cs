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

        public async Task<CandidateViewModel> GetCandidateById(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<CandidateViewModel>($"/api/Candidate/{id}");
            return await Task.FromResult(response);
        }

        public async Task<bool> AddCandidate(CandidateViewModel candidate)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/Candidate", candidate);

            return await Task.FromResult(response.IsSuccessStatusCode);
        }

        public async Task<bool> PutCandidate(CandidateViewModel candidate)
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/Candidate/{candidate.IdCandidate}", candidate);

            return await Task.FromResult(response.IsSuccessStatusCode);
        }

        public async Task<bool> CandidateExists(int candidateId)
        {
            var response = await _httpClient.GetAsync($"/api/Candidate/exist/{candidateId}");
            return await Task.FromResult(response.IsSuccessStatusCode);
        }

        public async Task<bool> DeleteCandidate(int candidateId)
        {
            var response = await _httpClient.DeleteAsync($"/api/Candidate/{candidateId}");
            return await Task.FromResult(response.IsSuccessStatusCode);
        }

        public async Task<List<CandidateViewModel>> GetCandidatesByElectionDistrictId(int electionId, int districtId)
        {
            var response = await _httpClient.GetFromJsonAsync<List<CandidateViewModel>>($"/api/Candidate/ElectionDistrictCandidates/{electionId}/{districtId}");
            return await Task.FromResult(response);
        }

        public bool IfCandidateListHasCandidate(int candidateId, List<CandidatePersonViewModel> candidates)
        {
            foreach(var candidate in candidates)
            {
                if(candidate.candidateViewModel.IdCandidate == candidateId)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
