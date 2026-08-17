using BankTellerSystem.Domain;
using BankTellerSystem.Server.Data;
using BankTellerSystem.Server.Queueing;
using Microsoft.EntityFrameworkCore;

namespace BankTellerSystem.Server.Services;

// Handles reading and updating currency exchange rates. Updates run through
// SerialOperationQueue for consistency with the other services and to avoid
// two simultaneous rate edits interleaving; reads don't need the queue since
// they don't mutate anything.
public class ExchangeRateService(IDbContextFactory<AppDbContext> dbFactory, SerialOperationQueue queue)
{
    // Currency-rate display screen: fetches all current rates.
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
            return rate;
        });
}