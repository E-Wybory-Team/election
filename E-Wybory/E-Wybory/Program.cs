using E_Wybory.Client.Pages;
using E_Wybory.Components;
using E_Wybory.Application;
using E_Wybory.Infrastructure;
using E_Wybory.ExtensionMethods;
using E_Wybory.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);

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


builder.ConfigureKestrel()
       .ConfigureAndAddDbContext();



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
