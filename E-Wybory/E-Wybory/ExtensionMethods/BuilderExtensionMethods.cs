using E_Wybory.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.EntityFrameworkCore.Internal;

namespace E_Wybory.ExtensionMethods {

    public static class BuilderExtensionMethods
    {
        public static WebApplicationBuilder ConfigureAndAddKestrel(this WebApplicationBuilder builder)
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


                        options.ListenAnyIP(8443, listenOptions =>
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

            string dbPassword;

            // Check if the environment is Production
            if (environment.IsProduction())
            {
                // Get the password from the environment variable DB_PASSWORD
                dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

            } else
            {
                //Get the password from users secret
                dbPassword = builder.Configuration["ConnectionStrings:ElectionDbConnection:DbPassword"];
            }

            if (!string.IsNullOrEmpty(dbPassword))
            {
                connectionString = connectionString.Replace("{DbPassword}", dbPassword);
            }
            //else throw new ArgumentException($"Cannot obtain password from source: {(environment.IsProduction() ? "PRODUCTION" : "DEVELOPMENT")}");  KOMENTARZ

            // Add the DbContext to the service collection using the (possibly) modified connection string
            builder.Services.AddDbContext<ElectionDbContext>(options =>
                options.UseMySQL(connectionString));

            return builder;
        }

        public static WebApplicationBuilder ConfigureAuth(this WebApplicationBuilder builder)
        {
            var rsaKey = RSA.Create();
            rsaKey.ImportRSAPrivateKey(File.ReadAllBytes("key"), out _);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    RoleClaimType = "Roles"
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = (ctx) =>
                    {
                        if (ctx.Request.Query.ContainsKey("t"))
                        {
                            ctx.Token = ctx.Request.Query["t"];
                        }
                        return Task.CompletedTask;
                    }
                };

                options.Configuration = new OpenIdConnectConfiguration
                {
                    SigningKeys = { new RsaSecurityKey(rsaKey) }
                };
                options.SaveToken = true;
                options.MapInboundClaims = false;
            });

            return builder;
        }
    }
}

