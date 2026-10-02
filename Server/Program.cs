using ArgosyUpdaterConsoleBLZ.Server;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

// The default content root is the current directory (%WINDIR%\System32 for a Windows Service),
// so always point it at the application folder where appsettings.json and wwwroot are published.
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

builder.Host.UseWindowsService();

// All settings (connection string, admin group, logging) live in the git-ignored appsettings.json next to the exe,
// see appsettings.template.json. It is not part of publish, so redeploys don't overwrite it.
// Environment variables (e.g. ConnectionStrings__ArgosyUpdater) override it.

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("ArgosyUpdater");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Connection string 'ConnectionStrings:ArgosyUpdater' is required (appsettings.json or environment variable ConnectionStrings__ArgosyUpdater).");

builder.Services.AddDbContext<MyDbContext>(options => options.UseSqlServer(connectionString));

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
