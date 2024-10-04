using Microsoft.Extensions.DependencyInjection;

namespace E_Wybory.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            //Add neccesery services for module here


            return services;
        }
    }
}
