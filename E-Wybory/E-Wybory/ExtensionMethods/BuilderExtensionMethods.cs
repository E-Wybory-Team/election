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
            else throw new ArgumentException($"Cannot obtain password from source: {(environment.IsProduction() ? "PRODUCTION" : "DEVELOPMENT")}");

            // Add the DbContext to the service collection using the (possibly) modified connection string
            builder.Services.AddDbContext<ElectionDbContext>(options =>
                options.UseMySQL(connectionString));

            return builder;
        }
    }

    public static class JWTMethods
    {
        public const int JWT_TOKEN_VALIDATION_MINS = 10;
        public static void CreateRSAPrivateKey()
        {
            var rsaKey = RSA.Create();
            var privateKey = rsaKey.ExportRSAPrivateKey();
            File.WriteAllBytes("key", privateKey);
        }

        public static string createToken(RSA rsaPrivateKey, string username)
        {
            var handler = new JsonWebTokenHandler();
            var key = new RsaSecurityKey(rsaPrivateKey);
            var token = handler.CreateToken(new SecurityTokenDescriptor()
            {
                Issuer = "https://localhost:8443",
                Subject = new ClaimsIdentity(new[]
                {
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim("name", username)
            }),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256),
                Expires = DateTime.Now.AddMinutes(JWT_TOKEN_VALIDATION_MINS)
            }) ;
            return token;
        }

        public static JsonWebKey CreateJwkPublic(RSA rsaPrivateKey) 
        {
            var publicKey = RSA.Create();
            publicKey.ImportRSAPublicKey(rsaPrivateKey.ExportRSAPublicKey(), out _);

            var key = new RsaSecurityKey(publicKey);
            return JsonWebKeyConverter.ConvertFromRSASecurityKey(key);
        }

        public static JsonWebKey CreateJwkPrivate(RSA rsaPrivateKey)
        {
            var key = new RsaSecurityKey(rsaPrivateKey);
            return JsonWebKeyConverter.ConvertFromRSASecurityKey(key);
        }

        public static string Authenticate(string email, string password)
        {
            if (email != "admin" || password != "admin")
            {
                return null;
            }
            CreateRSAPrivateKey();
            var rsaKey = RSA.Create();
            rsaKey.ImportRSAPrivateKey(File.ReadAllBytes("key"), out _);
            return createToken(rsaKey, email);
        }
    }

    public class AuthenticationRequest
    {
        public string email { set; get; }
        public string password { set; get; }
    }
}

