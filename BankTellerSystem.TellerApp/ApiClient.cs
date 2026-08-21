using System.Net;
using System.Net.Http.Json;

namespace BankTellerSystem.TellerApp;

// Thin wrapper around the WebAPI calls the teller app needs - one method per
// endpoint, no retry/caching. Errors from the server (400s) carry a plain-text
// body explaining what went wrong, so we surface that instead of a generic exception.
public class ApiClient(HttpClient httpClient)
{
    public async Task<List<AccountDto>> GetAccountsAsync(CancellationToken ct = default)
        => await httpClient.GetFromJsonAsync<List<AccountDto>>("api/accounts", ct) ?? [];

    public async Task<List<ExchangeRateDto>> GetExchangeRatesAsync(CancellationToken ct = default)
        => await httpClient.GetFromJsonAsync<List<ExchangeRateDto>>("api/exchange-rates", ct) ?? [];

    // Null means "no waiting ticket" (server responds 204 No Content).
    public async Task<TicketDto?> CallNextAsync(int counterId, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsync($"api/tickets/call-next?counterId={counterId}", null, ct);
        await EnsureSuccessAsync(response, ct);
        return response.StatusCode == HttpStatusCode.NoContent
            ? null
            : await response.Content.ReadFromJsonAsync<TicketDto>(cancellationToken: ct);
    }

    // Null means "this counter has nothing in progress" (also 204).
    public async Task<TicketDto?> CompleteCurrentAsync(int counterId, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsync($"api/tickets/complete-current?counterId={counterId}", null, ct);
        await EnsureSuccessAsync(response, ct);
        return response.StatusCode == HttpStatusCode.NoContent
            ? null
            : await response.Content.ReadFromJsonAsync<TicketDto>(cancellationToken: ct);
    }

    public async Task<TransactionDto> TransferAsync(int fromAccountId, int toAccountId, decimal amount, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/accounts/transfer",
            new { FromAccountId = fromAccountId, ToAccountId = toAccountId, Amount = amount }, ct);
        await EnsureSuccessAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<TransactionDto>(cancellationToken: ct))!;
    }

    public async Task<ExchangeRateDto> UpdateRateAsync(string currencyCode, decimal buyRate, decimal sellRate, CancellationToken ct = default)
    {
        var response = await httpClient.PutAsJsonAsync($"api/exchange-rates/{currencyCode}",
            new { CurrencyCode = currencyCode, BuyRate = buyRate, SellRate = sellRate }, ct);
        await EnsureSuccessAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<ExchangeRateDto>(cancellationToken: ct))!;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;

        var body = await response.Content.ReadAsStringAsync(ct);
        throw new InvalidOperationException(string.IsNullOrWhiteSpace(body) ? response.ReasonPhrase : body);
    }
}