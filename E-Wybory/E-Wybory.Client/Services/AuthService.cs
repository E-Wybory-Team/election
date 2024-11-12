using System.Net;
using System.Net.Http.Json;
using E_Wybory.Client.Components.Pages;
using E_Wybory.Client.ViewModels;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace E_Wybory.Client.Services
{
    public class AuthService(IHttpClientFactory clientFactory, AuthenticationStateProvider authenticationStateProvider) : IAuthService
    {
        private HttpClient _httpClient = clientFactory.CreateClient("ElectionHttpClient");
        private ElectionAuthStateProvider _stateProvider = authenticationStateProvider as ElectionAuthStateProvider;

        public async Task<bool> Login(LoginViewModel login)
        {
            var response =  await _httpClient.PostAsJsonAsync("/api/auth/login", login);

           return await AssignTokenFromRespone(response);
        }

        public async Task<bool> Register(RegisterViewModel register)
        { 
            var response = await _httpClient.PostAsJsonAsync("/api/auth/register", register);

            return await Task.FromResult(response.IsSuccessStatusCode);
        }

        public async Task<bool> Logout()
        {
            if(_stateProvider is not null)
            {
                await _stateProvider.MarkUserAsLoggedOut();

                var response = await _httpClient.PostAsJsonAsync("/api/auth/logout", new object());

                return response.IsSuccessStatusCode;
            }

            return false;
        }

        private async Task<bool> AssignTokenFromRespone(HttpResponseMessage response)
        {
            if (response is null) return false;

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadAsStringAsync();

                if (_stateProvider is not null && !string.IsNullOrEmpty(token))
                {
                    await _stateProvider.MarkUserAsAuthenticated(token);
                }

                return true;
            }

            return false;
        }

        public async Task<bool> RenewTokenClaims(UserInfoViewModel userInfo)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/renew-token", userInfo);

            return await AssignTokenFromRespone(response);
            
        }
    }
}
