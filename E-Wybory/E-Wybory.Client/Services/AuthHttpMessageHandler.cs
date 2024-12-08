﻿using Microsoft.JSInterop;
using System.Net.Http.Headers;

namespace E_Wybory.Client.Services
{
    public class AuthHttpMessageHandler : DelegatingHandler
    {
        private readonly IJSRuntime _jsRuntime;
        private const string TokenKey = "authToken";

        //      private readonly IMemoryCache _memoryCache; //check ? not really

        //https://referbruv.com/blog/using-imemorycache-for-token-caching-in-an-aspnet-core-application/
        //https://stackoverflow.com/questions/72519590/blazor-and-oauth-jwt-bearer-token-storage
        //redis???
        //registering IJSRuntime on Home page
        public AuthHttpMessageHandler(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", TokenKey);

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.Headers.TryGetValues("Authorization", out var authHeaders))
            {
                var authHeaderValue = authHeaders.FirstOrDefault();
                if (authHeaderValue != null && authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var newToken = authHeaderValue.Substring("Bearer ".Length).Trim();

                    if(token != newToken)
                        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", TokenKey, newToken);
                }
            }

            return response;
        }
    }
}
