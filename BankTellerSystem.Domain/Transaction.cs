namespace BankTellerSystem.Domain;

// A money transfer executed by a teller: FromAccount -> ToAccount.
public class Transaction
{
    public int Id { get; set; }

    public int FromAccountId { get; set; }

    public int ToAccountId { get; set; }

    public decimal Amount { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}