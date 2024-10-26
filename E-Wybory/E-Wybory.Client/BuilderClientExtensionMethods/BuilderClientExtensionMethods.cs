using E_Wybory.Client.Services;

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

            services.AddScoped(sp =>
            new HttpClient                    //builder.Configuration["FrontendUrl"]
            {
                BaseAddress = new Uri(endpointUri)
            });

            services.AddScoped<IAuthService, AuthService>();
        }
    }
}
