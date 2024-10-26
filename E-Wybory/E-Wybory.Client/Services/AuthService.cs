using System.Net.Http.Json;
using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public class AuthService(HttpClient httpClient) : IAuthService
    {
        private HttpClient _httpClient = httpClient;
        public async Task<string?> Login(LoginViewModel login)
        {
            var response =  await _httpClient.PostAsJsonAsync("/api/auth/login", login);

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadAsStringAsync();
                return token;
            }

            return null;
        }
    }

}
