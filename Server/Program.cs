using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting.WindowsServices;

// When running as a Windows Service the default content root is %WINDIR%\System32,
// so point it at the application folder (appsettings.json, wwwroot).
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default
});

builder.Host.UseWindowsService();

// Add services to the container.

// Windows authentication (Kerberos/NTLM) against the du.laus.hr domain.
// Every endpoint, including static files and the WASM payload, requires membership in the admin group.
var adminGroup = builder.Configuration["Authorization:AdminGroup"];
if (string.IsNullOrWhiteSpace(adminGroup))
    throw new InvalidOperationException("Configuration value 'Authorization:AdminGroup' is required.");

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireRole(adminGroup)
        .Build();
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Serves wwwroot of the Server and Client projects, including _framework (WASM runtime), with compression and fingerprinting.
app.MapStaticAssets();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
