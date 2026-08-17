using BankTellerSystem.Domain;

namespace BankTellerSystem.Server.Contracts;

// Response shape for a customer account.
public record AccountDto(int Id, string AccountNumber, string OwnerName, decimal Balance)
{
    public static AccountDto FromDomain(Account account) => new(
        account.Id,
        account.AccountNumber,
        account.OwnerName,
        account.Balance);
}