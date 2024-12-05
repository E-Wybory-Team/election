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
using Microsoft.OpenApi.Models;
using E_Wybory.Services.Interfaces;
using E_Wybory.Services;
using System;

namespace E_Wybory.ExtensionMethods {

    public static class BuilderExtensionMethods
    {
        public static WebApplicationBuilder ConfigureAndAddKestrel(this WebApplicationBuilder builder)
        {
            var environment = builder.Environment;

            //Kestrel server config
            if (environment.IsProduction())
            {

                builder.WebHost.UseKestrel((context, options) =>
                {

                    var kestrelConfig = context.Configuration.GetSection("Kestrel");

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


                        options.Configure(kestrelConfig).Endpoint("Https", listenOptions =>
                        {
                            listenOptions.HttpsOptions.ServerCertificate = certWithKey;
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

        public static WebApplicationBuilder ConfigureAuth(this WebApplicationBuilder builder, TokenValidationParameters validationParameters)
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
                options.TokenValidationParameters = validationParameters.Clone();
                options.TokenValidationParameters.IssuerSigningKey = new RsaSecurityKey(rsaKey);



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

            builder.Services.AddAuthorization(options => 
            {
                options.AddPolicy("2FAenabled", policy =>
                policy.RequireClaim("2FAenabled"));

                options.AddPolicy("2FAdisabled", policy =>
                policy.RequireClaim("2FAdisabled"));
            });

            return builder;
        }

        public static WebApplicationBuilder ConfigureSwagger(this WebApplicationBuilder builder)
        {
            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Election Web API", Version = "v1" });

                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Enter JWT token..."
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            Array.Empty<string>()
                        }
                    });
                });
            }
            return builder;
        }

        public static WebApplicationBuilder ConfigureEmailServiceSender(this WebApplicationBuilder builder)
        {
            if (builder.Environment.IsProduction())
            {
                var emailConnectionString = Environment.GetEnvironmentVariable("EMAIL_CONNECTION");
                if (!string.IsNullOrEmpty(emailConnectionString))
                {
                    builder.Configuration["EmailSettings:ConnectionString"] = emailConnectionString;
                }
            }

            builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddTransient<IEmailSenderService, EmailSenderService>();
            return builder;
        }

    }
}

