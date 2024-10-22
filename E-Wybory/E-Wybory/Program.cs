using E_Wybory.Client.Pages;
using E_Wybory.Components;
using E_Wybory.Application;
using E_Wybory.Infrastructure;
using E_Wybory.ExtensionMethods;
using E_Wybory.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

/*
 //Key creation
var rsaKey = RSA.Create();
var privateKey = rsaKey.ExportRSAPrivateKey();
File.WriteAllBytes("key", privateKey);
*/

var rsaKey = RSA.Create();
rsaKey.ImportRSAPrivateKey(File.ReadAllBytes("key"), out _);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("jwt")
    .AddJwtBearer("jwt", options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false
            };

            options.Events = new JwtBearerEvents()
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

            options.Configuration = new OpenIdConnectConfiguration()
            {
                SigningKeys =
                {
                    new RsaSecurityKey(rsaKey)
                }
            };

            options.MapInboundClaims = false;
        });

// Add services to the container.
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services
    .AddInfrastructure()
    .AddApplication();


// Conditionally configure Data Protection only in Release builds (C# preprocessor directive) TODO: Fix
//#if !DEBUG
//builder.Services.AddDataProtection()
//    .PersistKeysToFileSystem(new DirectoryInfo(@"/var/aspnetcore/dataprotection-keys"))
//    .SetApplicationName("E-Wybory");
//#endif


builder.ConfigureAndAddKestrel()
       .ConfigureAndAddDbContext();



builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(E_Wybory.Client._Imports).Assembly);

app.UseAuthentication();

app.MapGet("/userInfo", (HttpContext ctx) => ctx.User.FindFirst("sub")?.Value ?? "Empty");

app.MapGet("/jwt", () =>
{
    var handler = new JsonWebTokenHandler();
    var key = new RsaSecurityKey(rsaKey);
    var token = handler.CreateToken(new SecurityTokenDescriptor()
    {
        Issuer = "https://localhost:8443",
        Subject = new ClaimsIdentity(new[]
        {
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim("name", "coœ")
        }),
        SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256)
    }) ;
    return token;
});


app.MapGet("/jwk", () =>
{
    var publicKey = RSA.Create();
    publicKey.ImportRSAPublicKey(rsaKey.ExportRSAPublicKey(), out _);

    var key = new RsaSecurityKey(publicKey);
    return JsonWebKeyConverter.ConvertFromRSASecurityKey(key);
});


app.MapGet("/jwk-private", () =>
{
    var key = new RsaSecurityKey(rsaKey);
    return JsonWebKeyConverter.ConvertFromRSASecurityKey(key);
});

app.Run();
