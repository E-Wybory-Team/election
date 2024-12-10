using E_Wybory.Client.ViewModels;
using E_Wybory.Domain.Entities;
using System.Net.Http.Json;

namespace E_Wybory.Client.Services
{
    public class ElectionManagementService(IHttpClientFactory clientFactory) : IElectionManagementService
    {
        private HttpClient _httpClient = clientFactory.CreateClient("ElectionHttpClient");

        public async Task<List<ElectionViewModel>> Elections()
        {
            var response = await _httpClient.GetFromJsonAsync<List<ElectionViewModel>>("/api/Election");

            return await Task.FromResult(response);
        }

        public async Task<ElectionViewModel> GetElectionById(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<ElectionViewModel>($"/api/Election/{id}");
            return await Task.FromResult(response);
        }

        public async Task<bool> AddElection(ElectionViewModel election)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/Election", election);
            return await Task.FromResult(response.IsSuccessStatusCode);
        }

        public async Task<bool> PutElection(ElectionViewModel election)
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/Election/{election.IdElection}", election);
            return await Task.FromResult(response.IsSuccessStatusCode);
        }

        public async Task<bool> DeleteElection(int electionId)
        {
            var response = await _httpClient.DeleteAsync($"/api/Election/{electionId}");
            return await Task.FromResult(response.IsSuccessStatusCode);
        }

        public int? GetElectionTypeIdFromElection(int electionId, List<ElectionViewModel> elections)
        {
            var electionTypeId = elections
                .Where(d => d.IdElection == electionId)
                .Select(d => d.IdElectionType)
                .FirstOrDefault();

            return electionTypeId;
        }

        public async Task<List<ElectionViewModel>> GetActiveElections()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<ElectionViewModel>>("/api/Election/active");
                return response ?? new List<ElectionViewModel>();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine("No active elections found. Returning an empty list.");
                return new List<ElectionViewModel>();
            }
        }

        public async Task<List<ElectionViewModel>> GetNewestElections()
        {
            var response = await _httpClient.GetFromJsonAsync<List<ElectionViewModel>>("/api/Election/newest");
            return await Task.FromResult(response);
        }

        public async Task<List<ElectionViewModel>> GetNewestElectionsOfAllTypes()
        {
            var response = await _httpClient.GetFromJsonAsync<List<ElectionViewModel>>("/api/Election/newestAllTypes");
            return await Task.FromResult(response);
        }

        public async Task<ElectionViewModel> GetNewestElectionOfElectionType(int electionTypeId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ElectionViewModel>($"/api/Election/newest/{electionTypeId}");
                return response ?? new ElectionViewModel();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine("No elections of this type found which were started");
                return new ElectionViewModel();
            }
        }

        public async Task<List<ElectionViewModel>> GetElectionsOfType(int electionTypeId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<ElectionViewModel>>($"/api/Election/type/{electionTypeId}");
                return response ?? new List<ElectionViewModel>();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine("No elections of this type found. Returning an empty list.");
                return new List<ElectionViewModel>();
            }
        }

        public ElectionViewModel GetElectionOfTypeOfSingleElection(int electionTypeId, List<ElectionViewModel> elections)
        {
            foreach(var election in elections)
            {
                if(election.IdElectionType == electionTypeId)
                {
                    return election;
                }
            }
            return new ElectionViewModel();
        }

        public async Task<bool> ElectionOfTypeAtTimeExist(ElectionViewModel electionModel)
        {
            var response = await _httpClient.PutAsJsonAsync<ElectionViewModel>($"/api/Election/typeTime", electionModel);
            return await Task.FromResult(response.IsSuccessStatusCode);
        }

        public async Task<bool> ElectionIsNotSetToCandidate(int electionId)
        {
            var response = await _httpClient.GetAsync($"/api/Election/candidateNotSet/{electionId}");
            return await Task.FromResult(response.IsSuccessStatusCode);
        }


    }
}
