namespace BankTellerSystem.Domain;

// A currency's buy/sell rate, editable by a teller and shown live on the
// currency-rate display screen (Blazor app).
public class ExchangeRate
{
    public int Id { get; set; }

    // ISO currency code, e.g. "USD", "EUR", "CNY".
    public string CurrencyCode { get; set; } = string.Empty;

    public decimal BuyRate { get; set; }

    public decimal SellRate { get; set; }

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}