using E_Wybory.Client.ViewModels;
using System.Net.Http;
using System.Net.Http.Json;

namespace E_Wybory.Client.Services
{
    public class PersonManagementService(IHttpClientFactory clientFactory) : IPersonManagementService
    {
        private HttpClient _httpClient = clientFactory.CreateClient("ElectionHttpClient");

        public async Task<List<PersonViewModel>> People()
        {
            //Properly validate model before that 
            //All properties must be innitialize
            //register.idDistrict = 1;
            var response = await _httpClient.GetFromJsonAsync<List<PersonViewModel>>("/api/Person");

            return await Task.FromResult(response);
        }

        public async Task<int> GetPersonIdByPeselAsync(string pesel)
        {
            var response = await _httpClient.GetFromJsonAsync<List<PersonViewModel>>("/api/Person");

            var person = response?.FirstOrDefault(p => p.PESEL == pesel);
            return person?.IdPerson ?? 0;
        }

        public async Task<bool> AddPerson(PersonViewModel person)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/Person", person);

            return await Task.FromResult(response.IsSuccessStatusCode);
        }
    }
}
