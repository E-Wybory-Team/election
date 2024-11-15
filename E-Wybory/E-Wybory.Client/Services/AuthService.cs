using System.Net;
using System.Net.Http.Json;
using System.Text;
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

        private readonly Dictionary<int, string> userAuthenticatorKeys = new();
        private readonly Dictionary<int, bool> user2faEnabled = new();
        private readonly Dictionary<int, List<string>> userRecoveryCodes = new();

        public Task<bool> VerifyTwoFactorTokenAsync(int userId, string code)
        {
            return Task.FromResult(code == "123456");

        }

        public Task<int> CountRecoveryCodesAsync(int userId)
        {
            // return Task.FromResult(6);
            return Task.FromResult(userRecoveryCodes.ContainsKey(userId) ? userRecoveryCodes[userId].Count : 0);

        }

        public Task SetTwoFactorEnabledAsync(int userId, bool enabled)
        {
            user2faEnabled[userId] = enabled;
            return Task.CompletedTask;
        }

        public Task<IEnumerable<string>> GenerateNewTwoFactorRecoveryCodesAsync(int userId, int maxRecoveryCodes)
        {
            var codes = new List<string>();
            for (int i = 0; i < maxRecoveryCodes; i++)
            {
                codes.Add(Guid.NewGuid().ToString().Substring(0, 8));
            }
            userRecoveryCodes[userId] = codes;
            return Task.FromResult((IEnumerable<string>)codes);
        }

        public Task<string> GetAuthenticatorKeyAsync(int userId)
        {
            if (!userAuthenticatorKeys.ContainsKey(userId))
            {
                userAuthenticatorKeys[userId] = ConvertToBase32(Guid.NewGuid().ToByteArray()).Substring(0, 16);
            }
            return Task.FromResult(userAuthenticatorKeys[userId]);  
        }

        public Task<string> ResetAuthenticatorKeyAsync(int userId)
        {
            var newKey = ConvertToBase32(Guid.NewGuid().ToByteArray()).Substring(0, 16);
            userAuthenticatorKeys[userId] = newKey;
            return Task.FromResult(newKey);
        }

        private string ConvertToBase32(byte[] data)
        {
            const string base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            StringBuilder result = new StringBuilder((data.Length + 4) / 5 * 8);

            for (int i = 0; i < data.Length; i += 5)
            {
                int buffer = data[i] << 24;
                if (i + 1 < data.Length) buffer |= data[i + 1] << 16;
                if (i + 2 < data.Length) buffer |= data[i + 2] << 8;
                if (i + 3 < data.Length) buffer |= data[i + 3];
                if (i + 4 < data.Length) buffer |= data[i + 4] >> 8;

                for (int bitOffset = 35; bitOffset >= 0; bitOffset -= 5)
                {
                    result.Append(base32Chars[(buffer >> bitOffset) & 0x1F]);
                }
            }

            return result.ToString();
        }
    }
}
