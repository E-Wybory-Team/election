using E_Wybory.Client.BuilderClientExtensionMethods;
using E_Wybory.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddClientServices(builder.Configuration["ApiConfig:URL"]);


await builder.Build().RunAsync();
