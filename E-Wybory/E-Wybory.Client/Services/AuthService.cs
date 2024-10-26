using System.Net.Http.Json;
using E_Wybory.Client.Pages;
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

        public async Task<bool> Register(RegisterViewModel register)
        {
            //Properly validate model before that 
            //All properties must be innitialize
            register.idDistrict = 1;
            var response = await _httpClient.PostAsJsonAsync("/api/auth/register", register);

            return await Task.FromResult(response.IsSuccessStatusCode);
        }
    }

}
