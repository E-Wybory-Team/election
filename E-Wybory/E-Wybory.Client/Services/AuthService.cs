﻿using System.Net;
using System.Net.Http.Json;
using E_Wybory.Client.ViewModels;

namespace E_Wybory.Client.Services
{
    public class AuthService(IHttpClientFactory clientFactory) : IAuthService
    {
        private HttpClient _httpClient = clientFactory.CreateClient("ElectionHttpClient");
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
            var response = await _httpClient.PostAsJsonAsync("/api/auth/register", register);

            return await Task.FromResult(response.IsSuccessStatusCode);
        }
    }
}
