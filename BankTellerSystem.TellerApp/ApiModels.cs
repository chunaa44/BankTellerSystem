namespace BankTellerSystem.TellerApp;

public record AccountDto(int Id, string AccountNumber, string OwnerName, decimal Balance)
{
    // ComboBox shows an item via ToString() when DisplayMember isn't set.
    public override string ToString() => $"{AccountNumber} - {OwnerName} ({Balance:N0})";
}

public record ExchangeRateDto(int Id, string CurrencyCode, decimal BuyRate, decimal SellRate, DateTime UpdatedAtUtc)
{
    public override string ToString() => $"{CurrencyCode} (buy {BuyRate}, sell {SellRate})";
}

// Status comes across the wire as a number (System.Text.Json's default enum
// encoding) - we don't need to decode it client-side, just display the number.
public record TicketDto(int Id, string Number, int Status, DateTime CreatedAtUtc, int? CalledByCounterId, DateTime? CalledAtUtc);

public record TransactionDto(int Id, int FromAccountId, int ToAccountId, decimal Amount, DateTime CreatedAtUtc);