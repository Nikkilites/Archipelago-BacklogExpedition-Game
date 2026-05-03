using BEx.Web.Components;
using BEx.Web.Services;
using BEx.Core;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Determine data protection keys directory based on environment
var keyDirectory = builder.Environment.IsProduction()
    ? "/mnt/data-protection" // Render persistent disk mount path
    : "/home/appuser/.aspnet/DataProtection-Keys"; // Docker Desktop local path

// Ensure directory exists
Directory.CreateDirectory(keyDirectory);

builder.Services
    .AddDataProtection()
    .SetApplicationName("BEx-Web")
    .PersistKeysToFileSystem(new DirectoryInfo(keyDirectory))
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

builder.Services.AddSingleton<GameSessionManager>();
builder.Services.AddSingleton<BEx.Core.ILogger, WebLogger>();
builder.Services.AddHostedService<SessionCleanupService>();

builder.Services.AddScoped<IMessageService, WebMessageService>();
builder.Services.AddScoped<IDataLoader, HttpDataLoader>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
