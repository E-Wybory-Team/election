using E_Wybory.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
//using Microsoft.Extensions.Http;

namespace E_Wybory.Client.BuilderClientExtensionMethods
{
    public static class BuilderClientExtensionMethods
    {
        public static void AddClientServices(this IServiceCollection services, string? endpointUri = null)
        {
            endpointUri = string.IsNullOrEmpty(endpointUri) ||
                !Uri.IsWellFormedUriString(endpointUri, UriKind.Absolute) ? 
                "https://localhost:8443"  : 
                endpointUri;

            services.AddScoped<AuthHttpMessageHandler>();

            services.AddHttpClient("ElectionHttpClient", client =>
            {
                client.BaseAddress = new Uri(endpointUri);
            })
            .AddHttpMessageHandler<AuthHttpMessageHandler>();
           

            services.AddScoped<IAuthService, AuthService>();

            services.AddScoped<AuthenticationStateProvider, ElectionAuthStateProvider>();
        }
    }
}
