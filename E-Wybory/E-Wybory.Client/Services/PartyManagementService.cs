using E_Wybory.Client.Components;
using E_Wybory.Client.ViewModels;
using System;
using System.Net.Http.Json;

namespace E_Wybory.Client.Services
{
    public class PartyManagementService(IHttpClientFactory clientFactory) : IPartyManagementService
    {
        private HttpClient _httpClient = clientFactory.CreateClient("ElectionHttpClient");

        public async Task<List<PartyViewModel>> Parties()
        {

            var response = await _httpClient.GetFromJsonAsync<List<PartyViewModel>>("/api/Party");

            return await Task.FromResult(response);
        }

        public string? GetPartyNameById(int partyId, List<PartyViewModel> parties)
        {
            var party = parties
                .Where(p => p.IdParty == partyId)
                .Select(p => p.PartyName)
                .FirstOrDefault();

            return party;
        }

        public async Task<PartyViewModel> GetPartyById(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<PartyViewModel>($"/api/Party/{id}");
            return await Task.FromResult(response);
        }

        public async Task<List<PartyViewModel>> GetFilteredParties (int? electionTypeId)
        {
            var response = await _httpClient.GetFromJsonAsync<List<PartyViewModel>>($"/api/Party/Filter/?electionTypeId={electionTypeId}");
            return response;
        }

        public async Task<bool> AddParty(PartyViewModel party)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/Party", party);

            return await Task.FromResult(response.IsSuccessStatusCode);
        }

        public async Task<bool> PutParty(PartyViewModel party)
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/Party/{party.IdParty}", party);

            return await Task.FromResult(response.IsSuccessStatusCode);
        }

        public async Task<bool> DeleteParty(int partyId)
        {
            var response = await _httpClient.DeleteAsync($"/api/Party/{partyId}");
            return await Task.FromResult(response.IsSuccessStatusCode);
        }

        public async Task<bool> PartyExists(int partyId)
        {
            var response = await _httpClient.GetAsync($"/api/Party/exist/{partyId}");
            return await Task.FromResult(response.IsSuccessStatusCode);
        }
    }
}
