﻿using E_Wybory.Client.ViewModels;
using System.Net.Http.Json;

namespace E_Wybory.Client.Services
{
    public class ElectionManagementService(IHttpClientFactory clientFactory) : IElectionManagementService
    {
        private HttpClient _httpClient = clientFactory.CreateClient("ElectionHttpClient");

        public async Task<List<ElectionViewModel>> Elections()
        {
            //Properly validate model before that 
            //All properties must be innitialize
            //register.idParty = 1;
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

    }
}
