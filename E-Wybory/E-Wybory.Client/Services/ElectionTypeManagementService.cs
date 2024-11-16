using E_Wybory.Client.ViewModels;
using System.Net.Http.Json;

namespace E_Wybory.Client.Services
{
    public class ElectionTypeManagementService(IHttpClientFactory clientFactory) : IElectionTypeManagementService
    {
        private HttpClient _httpClient = clientFactory.CreateClient("ElectionHttpClient");

        public async Task<List<ElectionTypeViewModel>> ElectionTypes()
        {
            //Properly validate model before that 
            //All properties must be innitialize
            var response = await _httpClient.GetFromJsonAsync<List<ElectionTypeViewModel>>("/api/ElectionType");

            return await Task.FromResult(response);
        }

        public string? GetElectionTypeNameById(int electionTypeId, List<ElectionTypeViewModel> electionTypes)
        {
            var party = electionTypes
                .Where(p => p.IdElectionType == electionTypeId)
                .Select(p => p.ElectionTypeName)
                .FirstOrDefault();

            return party;
        }

    }
}
