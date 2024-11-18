using System.Net;
using System.Net.Http.Json;
using System.Text;
using E_Wybory.Application.DTOs;
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

        //https://learn.microsoft.com/en-us/aspnet/core/security/authentication/mfa?view=aspnetcore-8.0
        //https://learn.microsoft.com/en-us/aspnet/core/blazor/security/server/qrcodes-for-authenticator-apps?view=aspnetcore-8.0
        //https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-enable-qrcodes?view=aspnetcore-8.0#using-a-different-qr-code-library

        //private readonly Dictionary<int, string> userAuthenticatorKeys = new();
        //private readonly Dictionary<int, bool> user2faEnabled = new();
        //private readonly Dictionary<int, List<string>> userRecoveryCodes = new();

        public async Task<bool> VerifyTwoFactorTokenAsync(int userId, string code)
        {
            var req2fa = new TwoFactorAuthVerifyRequest() { UserId = userId, Code = code };
            var response = await _httpClient.PostAsJsonAsync("/api/auth/verify-2fa-first", req2fa);

            return response.IsSuccessStatusCode;

        }

        public async Task<int> CountRecoveryCodesAsync(int userId)
        {
            // return Task.FromResult(6);
            //return Task.FromResult(userRecoveryCodes.ContainsKey(userId) ? userRecoveryCodes[userId].Count : 0);

            var response = await _httpClient.GetAsync($"api/auth/count-rec-codes/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var countResponse = await response.Content.ReadFromJsonAsync<CountResponse>();
                return Convert.ToInt32(countResponse?.Count);
            }

            return 0;
        }

        public async Task<bool> SetTwoFactorEnabledAsync(int userId, bool enabled)
        {
            var enable2fa = new TwoFactorEnabledRequest() { UserId = userId, IsEnabled = enabled };
            var response = await _httpClient.PostAsJsonAsync("/api/auth/enable-2fa", enable2fa);

            return await Task.FromResult(response.IsSuccessStatusCode);
        }

        public async Task<IEnumerable<string>> GenerateNewTwoFactorRecoveryCodesAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"api/auth/gen-rec-codes/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var recCodes = await response.Content.ReadFromJsonAsync<IEnumerable<string>>();
                return recCodes!;
            }

            return new List<string>();
        }

        public async Task<string> GetAuthenticatorKeyAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"api/auth/get-auth-key/{userId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return string.Empty;

            //return ConvertToBase32(Guid.NewGuid().ToByteArray()).Substring(0, 16);
        }

        public async Task<string> ResetAuthenticatorKeyAsync(int userId)
        {
            var response = await _httpClient.PostAsync($"api/auth/reset-auth-key/{userId}", null);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return string.Empty;
        }

    }
}
