using BankTellerSystem.RateDisplay.Components;
using BankTellerSystem.RateDisplay.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Typed HttpClient for the initial rate snapshot; base address comes from
// appsettings.json so it's easy to point at a different server/port later.
builder.Services.AddHttpClient<ExchangeRateApiClient>((_, client) =>
{
    var serverBaseUrl = builder.Configuration["ServerBaseUrl"]
        ?? throw new InvalidOperationException("ServerBaseUrl missing from appsettings.json.");
    client.BaseAddress = new Uri(serverBaseUrl);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();