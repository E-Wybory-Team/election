using E_Wybory.Client.ViewModels;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace E_Wybory.Client.Services
{
    public class UserTypeManagementService(IHttpClientFactory clientFactory): IUserTypeManagementService
    {
            private HttpClient _httpClient = clientFactory.CreateClient("ElectionHttpClient");

            public async Task<List<UserTypeViewModel>> UserTypes()
            {
                var response = await _httpClient.GetFromJsonAsync<List<UserTypeViewModel>>("/api/UserType");
                return await Task.FromResult(response);
            }

            public async Task<List<UserTypeViewModel>> GetUserTypesOfGroup(int groupId)
            {
                var response = await _httpClient.GetFromJsonAsync<List<UserTypeViewModel>>($"/api/UserType/group/{groupId}");
                return await Task.FromResult(response);
            }

        public async Task<UserTypeViewModel> GetUserTypeById(int id)
            {
                var response = await _httpClient.GetFromJsonAsync<UserTypeViewModel>($"/api/UserType/{id}");
                return await Task.FromResult(response);
            }

            public async Task<string> GetUserTypeNameById(int id)
            {
                try
                {
                    var response = await _httpClient.GetFromJsonAsync<UserTypeViewModel>($"/api/UserType/{id}");
                    var userType = await Task.FromResult(response);
                    return await Task.FromResult(userType.UserTypeName);
                }
                catch (Exception ex)
                {
                return String.Empty;
                }
            }
    }
}
