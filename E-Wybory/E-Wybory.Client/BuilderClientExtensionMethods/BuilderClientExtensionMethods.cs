using E_Wybory.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Encodings.Web;

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
            services.AddScoped<IFilterWrapperManagementService, FilterWrapperManagementService>();
            services.AddScoped<IConstituencyManagementService, ConstituencyManagementService>();
            services.AddScoped<IElectionUserManagementService, ElectionUserManagementService>();
            services.AddScoped<IVoterManagementService, VoterManagementService>();
            services.AddScoped<IElectionVoterManagementService, ElectionVoterManagementService>();
            services.AddScoped<IVoteManagementService, VoteManagementService>();
            services.AddScoped<IUserTypeManagementService, UserTypeManagementService>();
            services.AddScoped<IUserTypeSetsManagementService, UserTypesSetsManagementService>();
            services.AddScoped<AuthenticationStateProvider, ElectionAuthStateProvider>();
        }
    }
}
