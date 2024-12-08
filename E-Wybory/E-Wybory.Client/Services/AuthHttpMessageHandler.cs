using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Net.Http.Headers;

namespace E_Wybory.Client.Services
{
    public class AuthHttpMessageHandler : DelegatingHandler
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly IServiceProvider _serviceProvider;
        private const string TokenKey = "authToken";

        private readonly static bool shouldRedirect = false;

        public AuthHttpMessageHandler(IJSRuntime jsRuntime, IServiceProvider serviceProvider)
        {
            _jsRuntime = jsRuntime;
            _serviceProvider = serviceProvider;
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

                    if (token != newToken)
                        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", TokenKey, newToken);
                }
            }

            if (shouldRedirect && (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden))
            {
                bool wasLoggedOut = false;

                using (var scope = _serviceProvider.CreateScope())
                {

                    var scopedAuthService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                    var scopedAuthStateProvider = scope.ServiceProvider.GetRequiredService<AuthenticationStateProvider>();
                    if (scopedAuthService is not null && scopedAuthStateProvider is not null)
                    {
                        var user = (await scopedAuthStateProvider.GetAuthenticationStateAsync()).User;
                        if (user?.Identity?.IsAuthenticated ?? false)
                        {
                            wasLoggedOut = await scopedAuthService.Logout();
                        }
                    }

                    var navigationManager = _serviceProvider.GetRequiredService<NavigationManager>();
                    if (navigationManager is not null) navigationManager.NavigateTo("/login");
                }
                if (wasLoggedOut)
                    await _jsRuntime.InvokeVoidAsync("location.reload");
            }

            return response;
        }
    }
}
