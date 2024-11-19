using E_Wybory.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Encodings.Web;
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

            services.AddSingleton<UrlEncoder>(UrlEncoder.Default);


            services.AddScoped<AuthenticationStateProvider, ElectionAuthStateProvider>();
            services.AddAuthorizationCore();

            services.AddScoped<IAuthService, AuthService>();

            services.AddScoped<IDistrictManagementService, DistrictManagementService>();
            services.AddScoped<IPartyManagementService, PartyManagementService>();
            services.AddScoped<ICandidateManagementService, CandidateManagementService>();
            services.AddScoped<IPersonManagementService, PersonManagementService>();
            services.AddScoped<IElectionManagementService, ElectionManagementService>();
            services.AddScoped<IVoivodeshipManagementService, VoivodeshipManagementService>();
            services.AddScoped<IProvinceManagementService, ProvinceManagementService>();
            services.AddScoped<ICountyManagementService, CountyManagementService>();
            services.AddScoped<IElectionTypeManagementService, ElectionTypeManagementService>();
        }
    }
}
