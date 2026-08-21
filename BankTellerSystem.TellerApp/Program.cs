using System.Text.Json;

namespace BankTellerSystem.TellerApp;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var serverBaseUrl = ReadServerBaseUrl();
        var httpClient = new HttpClient { BaseAddress = new Uri(serverBaseUrl) };
        var apiClient = new ApiClient(httpClient);

        Application.Run(new TellerForm(apiClient));
    }

    private static string ReadServerBaseUrl()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var json = File.ReadAllText(path);

        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("ServerBaseUrl").GetString()
            ?? throw new InvalidOperationException("ServerBaseUrl missing from appsettings.json.");
    }
}