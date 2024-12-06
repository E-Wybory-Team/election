using E_Wybory.Client.ViewModels;
using E_Wybory.Domain.Entities;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using static E_Wybory.Client.Components.Pages.VotersList;

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
            var response = await _httpClient.GetFromJsonAsync<int>($"/api/Person/idFromPesel/{pesel}");
            return await Task.FromResult(response);
        }

        public async Task<bool> AddPerson(PersonViewModel person)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/Person", person);

            return await Task.FromResult(response.IsSuccessStatusCode);
        }

        public async Task<bool> PutPerson(PersonViewModel person)
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/Person/{person.IdPerson}", person);

            return await Task.FromResult(response.IsSuccessStatusCode);
        }

        public async Task<PersonViewModel> GetPersonById(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<PersonViewModel>($"/api/Person/{id}");
            return await Task.FromResult(response);
        }

        public async Task<String> GetPersonNameSurnameById(int id)
        {
            var person = await GetPersonById(id);

            return await Task.FromResult($"{person.Name} {person.Surname} - wiek: {CountPersonAge(person.BirthDate)}");
        }

        public async Task<String> GetPersonNameSurnameWithoutAgeById(int id)
        {
            var person = await GetPersonById(id);
            return await Task.FromResult($"{person.Name} {person.Surname}");
        }


        public async Task<PersonViewModel> GetPersonIdByIdElectionUser(int electionUserId)
        {
            var response = await _httpClient.GetFromJsonAsync<PersonViewModel>($"/api/Person/fromUser/{electionUserId}");
            return await Task.FromResult(response);
        }
    

    public int CountPersonAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;

            if (birthDate.Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }
    }
}
