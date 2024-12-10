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

var handler = new JsonWebTokenHandler();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

if (!builder.Environment.IsProduction())
    builder.Logging.AddDebug();


builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services
    .AddInfrastructure()
    .AddApplication();


builder.ConfigureAndAddKestrel()
       .ConfigureAndAddDbContext();

//Added JWT Bearer configuration
builder.ConfigureAuth(validationParameters);


//Add client services
builder.Services.AddClientServices(builder.Configuration["Kestrel:Endpoints:Https:Url"]);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.ConfigureSwagger();
builder.ConfigureEmailServiceSender();
builder.Services.AddSingleton<RSA>(rsaKey);
builder.Services.AddSingleton<TokenValidationParameters>(validationParameters);
builder.Services.AddSingleton<IJWTService,TokenService>();
builder.Services.AddSingleton<JsonWebTokenHandler>(handler);


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<TokenRenewalMiddleware>();

app.Run();
