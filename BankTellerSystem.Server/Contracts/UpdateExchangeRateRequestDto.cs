namespace BankTellerSystem.Server.Contracts;

// Request body for a teller updating a currency's buy/sell rate.
public record UpdateExchangeRateRequestDto(string CurrencyCode, decimal BuyRate, decimal SellRate);