using System.Net.Http.Json;

namespace BankTellerSystem.RateDisplay.Services;

// Thin wrapper around the one REST call this screen needs: the initial
// snapshot of rates before the SignalR connection takes over for updates.
public class ExchangeRateApiClient(HttpClient httpClient)
{
    public async Task<List<ExchangeRateDto>> GetAllAsync(CancellationToken ct = default)
        => await httpClient.GetFromJsonAsync<List<ExchangeRateDto>>("api/exchange-rates", ct) ?? [];
}