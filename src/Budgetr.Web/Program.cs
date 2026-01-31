using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Budgetr.Web;
using Budgetr.Web.Services;
using Budgetr.Shared.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register services
builder.Services.AddScoped<IStorageService, BrowserStorageService>();
builder.Services.AddScoped<ITimeTrackingService, TimeTrackingService>();

var host = builder.Build();

// Load saved data on startup
var timeService = host.Services.GetRequiredService<ITimeTrackingService>();
await timeService.LoadAsync();

await host.RunAsync();
