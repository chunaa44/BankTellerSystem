namespace BankTellerSystem.Server.Contracts;

// Request body for a teller-initiated account-to-account transfer.
public record TransferRequestDto(int FromAccountId, int ToAccountId, decimal Amount);