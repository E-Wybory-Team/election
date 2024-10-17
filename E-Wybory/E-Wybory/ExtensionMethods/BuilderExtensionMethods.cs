using E_Wybory.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace E_Wybory.ExtensionMethods {

    public static class BuilderExtensionMethods
    {
        public static WebApplicationBuilder ConfigureKestrel(this WebApplicationBuilder builder)
        {
            var environment = builder.Environment;

            //Kestrel server config
            if (environment.IsProduction())
            {

                builder.WebHost.ConfigureKestrel((context, options) =>
                {



                    var kestrelConfig = context.Configuration.GetSection("Kestrel");

                    options.Configure(kestrelConfig);

                    var certPath = Environment.GetEnvironmentVariable("CERTIFICATE_PATH_VOTING");
                    var certKeyPath = Environment.GetEnvironmentVariable("CERTIFICATE_KEY_PATH_VOTING");

                    // Check if certificate path and key path are configured
                    if (!string.IsNullOrEmpty(certPath) && !string.IsNullOrEmpty(certKeyPath))
                    {
                        //Added X509Certificate2 conversion
                        var certPem = System.IO.File.ReadAllText(certPath);
                        var keyPem = System.IO.File.ReadAllText(certKeyPath);

                        // Create the X509Certificate2 from both
                        var certWithKey = X509Certificate2.CreateFromPem(certPem, keyPem);


                        options.ListenAnyIP(443, listenOptions =>
                        {
                            listenOptions.UseHttps(certWithKey);
                        });
                    }


                });
            }
            return builder;
        }

        public static WebApplicationBuilder ConfigureAndAddDbContext(this WebApplicationBuilder builder)
        {
            var environment = builder.Environment;

            // Get the connection string from the configuration
            var connectionString = builder.Configuration.GetConnectionString("ElectionDbConnection");

            if (connectionString == null) throw new ArgumentNullException("Cannot localize the connection string");

            // Check if the environment is Production
            if (environment.IsProduction())
            {
                // Get the password from the environment variable DB_PASSWORD
                var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

                // Replace the {DbPassword} placeholder in the connection string with the actual password
                if (!string.IsNullOrEmpty(dbPassword))
                {
                    connectionString = connectionString.Replace("{DbPassword}", dbPassword);
                }
            }

            // Add the DbContext to the service collection using the (possibly) modified connection string
            builder.Services.AddDbContext<ElectionDbContext>(options =>
                options.UseMySQL(connectionString));

            return builder;
        }
    }
}

