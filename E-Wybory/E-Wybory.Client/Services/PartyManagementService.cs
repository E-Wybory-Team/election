using E_Wybory.Client.ViewModels;
using System.Net.Http.Json;

namespace E_Wybory.Client.Services
{
    public class PartyManagementService(IHttpClientFactory clientFactory) : IPartyManagementService
    {
        private HttpClient _httpClient = clientFactory.CreateClient("ElectionHttpClient");

        public async Task<List<PartyViewModel>> Parties()
        {
            //Properly validate model before that 
            //All properties must be innitialize
            //register.idParty = 1;
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

    }
}
