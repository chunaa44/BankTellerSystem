using BankTellerSystem.Domain;

namespace BankTellerSystem.Server.Contracts;

// Response shape for a completed transfer.
public record TransactionDto(int Id, int FromAccountId, int ToAccountId, decimal Amount, DateTime CreatedAtUtc)
{
    public static TransactionDto FromDomain(Transaction transaction) => new(
        transaction.Id,
        transaction.FromAccountId,
        transaction.ToAccountId,
        transaction.Amount,
        transaction.CreatedAtUtc);
}