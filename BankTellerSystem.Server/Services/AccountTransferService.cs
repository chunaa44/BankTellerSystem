using BankTellerSystem.Domain;
using BankTellerSystem.Server.Data;
using BankTellerSystem.Server.Queueing;
using Microsoft.EntityFrameworkCore;

namespace BankTellerSystem.Server.Services;

// Handles teller-initiated money transfers between two accounts.
// Runs through SerialOperationQueue so two transfers touching the same
// account can't race and produce an incorrect balance.
public class AccountTransferService(IDbContextFactory<AppDbContext> dbFactory, SerialOperationQueue queue)
{
    // Moves Amount from FromAccountId to ToAccountId and records a Transaction.
    // Throws InvalidOperationException for bad input (missing account, same
    // account, non-positive amount, insufficient funds) - the caller/controller
    // translates that into an appropriate HTTP response.
    public Task<Transaction> TransferAsync(int fromAccountId, int toAccountId, decimal amount, CancellationToken ct = default)
        => queue.Enqueue(async () =>
        {
            if (fromAccountId == toAccountId)
                throw new InvalidOperationException("Cannot transfer to the same account.");

            if (amount <= 0)
                throw new InvalidOperationException("Transfer amount must be positive.");

            await using var db = await dbFactory.CreateDbContextAsync(ct);

            var from = await db.Accounts.FindAsync([fromAccountId], ct)
                ?? throw new InvalidOperationException($"Account {fromAccountId} not found.");

            var to = await db.Accounts.FindAsync([toAccountId], ct)
                ?? throw new InvalidOperationException($"Account {toAccountId} not found.");

            if (from.Balance < amount)
                throw new InvalidOperationException("Insufficient funds.");

            from.Balance -= amount;
            to.Balance += amount;

            var transaction = new Transaction
            {
                FromAccountId = fromAccountId,
                ToAccountId = toAccountId,
                Amount = amount
            };
            db.Transactions.Add(transaction);

            await db.SaveChangesAsync(ct);
            return transaction;
        });
}