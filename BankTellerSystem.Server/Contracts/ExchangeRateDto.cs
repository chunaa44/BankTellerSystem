using BankTellerSystem.Domain;

namespace BankTellerSystem.Server.Contracts;

// Response shape shown on the currency-rate display (Blazor app).
public record ExchangeRateDto(int Id, string CurrencyCode, decimal BuyRate, decimal SellRate, DateTime UpdatedAtUtc)
{
    public static ExchangeRateDto FromDomain(ExchangeRate rate) => new(
        rate.Id,
        rate.CurrencyCode,
        rate.BuyRate,
        rate.SellRate,
        rate.UpdatedAtUtc);
}