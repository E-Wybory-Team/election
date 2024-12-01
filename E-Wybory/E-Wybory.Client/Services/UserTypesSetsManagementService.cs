using E_Wybory.Client.ViewModels;
using E_Wybory.Domain.Entities;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace E_Wybory.Client.Services
{
    public class UserTypesSetsManagementService(IHttpClientFactory clientFactory): IUserTypeSetsManagementService
    {
            private HttpClient _httpClient = clientFactory.CreateClient("ElectionHttpClient");

            public async Task<List<UserTypeSetViewModel>> UserTypesSets()
            {
                var response = await _httpClient.GetFromJsonAsync<List<UserTypeSetViewModel>>("/api/UserTypesSets");
                return await Task.FromResult(response);
            }

            public async Task<List<CommissionerViewModel>> CommissionersOfDistrict(int districtId)
            {
                var response = await _httpClient.GetFromJsonAsync<List<CommissionerViewModel>>($"/api/UserTypesSets/commissioners/{districtId}");
                return await Task.FromResult(response);
            }

            public async Task<bool> AddUserTypeSet(UserTypeSetViewModel userTypeSetViewModel)
            {
                var response = await _httpClient.PostAsJsonAsync($"/api/UserTypesSets", userTypeSetViewModel);
                return await Task.FromResult(response.IsSuccessStatusCode);
            }

            public async Task<bool> DeleteUserTypeSet(int userTypeSetId)
            {
                var response = await _httpClient.DeleteAsync($"/api/UserTypesSets/{userTypeSetId}");
                return await Task.FromResult(response.IsSuccessStatusCode);
            }

            public async Task<bool> UserWithTypeGroupExists(int userTypeId, int electionUserId)
            {
                var response = await _httpClient.GetFromJsonAsync<bool>($"/api/UserTypesSets/typeGroupUser/{userTypeId}/{electionUserId}");
                return response;
            }

            public async Task<UserTypeSetViewModel> SetWithTypeGroupExists(int typeGroupId, int electionUserId)
            {
                var response = await _httpClient.GetFromJsonAsync<UserTypeSetViewModel>($"/api/UserTypesSets/setGroupUser/{typeGroupId}/{electionUserId}");
                return await Task.FromResult(response);
            }

            public async Task<bool> PutUserTypeSet(UserTypeSetViewModel userTypeSetModel)
            {
                var response = await _httpClient.PutAsJsonAsync($"/api/UserTypesSets/{userTypeSetModel.IdUserTypeSet}", userTypeSetModel);
                return await Task.FromResult(response.IsSuccessStatusCode);
            }

    }
}
