using Microsoft.JSInterop;
using System.Net.Http.Headers;

namespace E_Wybory.Client.Services
{
    public class AuthHttpMessageHandler : DelegatingHandler
    {
        private readonly IJSRuntime _jsRuntime;
        private const string TokenKey = "authToken";

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

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
