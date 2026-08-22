namespace BankTellerSystem.RateDisplay.Services;

// Shape used both for the REST snapshot and for SignalR "RateUpdated" pushes.
// No Id - the display keys rows by CurrencyCode, and the hub push doesn't
// carry one anyway (extra/missing JSON properties are ignored on either side).
public record ExchangeRateDto(string CurrencyCode, decimal BuyRate, decimal SellRate, DateTime UpdatedAtUtc);