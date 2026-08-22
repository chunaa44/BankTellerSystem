// BankTellerSystem.Server/Services/ExchangeRateService.cs
using BankTellerSystem.Domain;
using BankTellerSystem.Server.Data;
using BankTellerSystem.Server.Queueing;
using BankTellerSystem.Server.Realtime;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BankTellerSystem.Server.Services;

// Handles reading and updating currency exchange rates. Updates run through
// SerialOperationQueue for consistency with the other services and to avoid
// two simultaneous rate edits interleaving; reads don't need the queue since
// they don't mutate anything. Every successful update is also pushed out over
// SignalR so the Blazor rate-display screen updates itself in real time.
public class ExchangeRateService(
    IDbContextFactory<AppDbContext> dbFactory,
    SerialOperationQueue queue,
    IHubContext<ExchangeRateHub> hub)
{
    // Currency-rate display screen: fetches all current rates. Used for the
    // initial page load only - after that, updates arrive via SignalR.
    public async Task<List<ExchangeRate>> GetAllAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.ExchangeRates.OrderBy(r => r.CurrencyCode).ToListAsync(ct);
    }

    // Teller: updates the buy/sell rate for a currency. Throws if the
    // currency code isn't already seeded/known.
    public Task<ExchangeRate> UpdateRateAsync(string currencyCode, decimal buyRate, decimal sellRate, CancellationToken ct = default)
        => queue.Enqueue(async () =>
        {
            if (buyRate <= 0 || sellRate <= 0)
                throw new InvalidOperationException("Rates must be positive.");

            if (buyRate > sellRate)
                throw new InvalidOperationException("Buy rate cannot exceed sell rate.");

            await using var db = await dbFactory.CreateDbContextAsync(ct);

            var rate = await db.ExchangeRates.FirstOrDefaultAsync(r => r.CurrencyCode == currencyCode, ct)
                ?? throw new InvalidOperationException($"Currency '{currencyCode}' not found.");

            rate.BuyRate = buyRate;
            rate.SellRate = sellRate;
            rate.UpdatedAtUtc = DateTime.UtcNow;

            await db.SaveChangesAsync(ct);

            // Tell every connected display screen about the new rate.
            await hub.Clients.All.SendAsync("RateUpdated", new
            {
                rate.CurrencyCode,
                rate.BuyRate,
                rate.SellRate,
                rate.UpdatedAtUtc
            }, ct);

            return rate;
        });
}