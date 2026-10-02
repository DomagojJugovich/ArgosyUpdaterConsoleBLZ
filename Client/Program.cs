using ArgosyUpdaterConsoleBLZ.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Syncfusion.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<MachineStatsService>();

builder.Services.AddSyncfusionBlazor();
// License key is read from wwwroot/appsettings.json (not in source control, see appsettings.template.json).
var syncfusionLicenseKey = builder.Configuration["Syncfusion:LicenseKey"];
if (string.IsNullOrWhiteSpace(syncfusionLicenseKey))
    Console.Error.WriteLine("Syncfusion:LicenseKey is missing in wwwroot/appsettings.json; Syncfusion will show a license banner.");
else
    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncfusionLicenseKey);


await builder.Build().RunAsync();
