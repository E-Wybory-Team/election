using E_Wybory.Client.Components.Pages;
using E_Wybory.Client.Components;
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
using E_Wybory.Client.BuilderClientExtensionMethods;
using E_Wybory.Services;
using Microsoft.OpenApi.Models;
using E_Wybory.Middleware;
using System.Text.Json.Serialization;


var rsaKey = RSA.Create();



var builder = WebApplication.CreateBuilder(args);

TokenValidationParameters validationParameters = new TokenValidationParameters
{
    ValidateAudience = false,
    ValidateIssuer = false,
    ValidateLifetime = true,
    RoleClaimType = "Roles",
    ValidateIssuerSigningKey = true,
    ClockSkew = TimeSpan.Zero,
};

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

//Added JWT Bearer configuration
builder.ConfigureAuth(validationParameters);

//builder.Services.AddAuthorizationCore();

//Add client services
builder.Services.AddClientServices(builder.Configuration["Kestrel:Endpoints:Https:Url"]);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.ConfigureSwagger();
builder.ConfigureEmailServiceSender();
builder.Services.AddSingleton<RSA>(rsaKey);
builder.Services.AddSingleton<TokenValidationParameters>(validationParameters);
builder.Services.AddSingleton<IJWTService,TokenService>();


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
    .AddInteractiveWebAssemblyRenderMode();
    //.AddAdditionalAssemblies(typeof(E_Wybory.Client._Imports).Assembly);    // TODO: commented because error 'Assembly already defined'            

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<TokenRenewalMiddleware>();


//app.MapGet("/userInfo", (HttpContext ctx) => ctx.User.FindFirst("sub")?.Value ?? "Empty");

/*
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

*/

app.Run();
