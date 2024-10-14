using E_Wybory.Client.Pages;
using E_Wybory.Components;
using E_Wybory.Application;
using E_Wybory.Infrastructure;
using System;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services
    .AddInfrastructure()
    .AddApplication();





// Conditionally configure Data Protection only in Release builds (C# preprocessor directive)
//#if !DEBUG
//builder.Services.AddDataProtection()
//    .PersistKeysToFileSystem(new DirectoryInfo(@"/var/aspnetcore/dataprotection-keys"))
//    .SetApplicationName("E-Wybory");
//#endif


//Configure Kestrel for https with certificates forrelease build
var environment = builder.Environment;

if (environment.IsProduction())
{

    builder.WebHost.ConfigureKestrel((context, options) =>
    {

        

        var kestrelConfig = context.Configuration.GetSection("Kestrel");

        // Always configure the endpoints
        options.Configure(kestrelConfig);

        var certPath = Environment.GetEnvironmentVariable("CERTIFICATE_PATH_VOTING");
        var certKeyPath = Environment.GetEnvironmentVariable("CERTIFICATE_KEY_PATH_VOTING");

        // Check if certificate path and key path are configured
        if (!string.IsNullOrEmpty(certPath) && !string.IsNullOrEmpty(certKeyPath))
        {
            options.ListenAnyIP(443, listenOptions =>
            {
                listenOptions.UseHttps(certPath, certKeyPath);
            });
        }


    });
}



builder.Services.AddControllers();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(E_Wybory.Client._Imports).Assembly);

app.Run();
