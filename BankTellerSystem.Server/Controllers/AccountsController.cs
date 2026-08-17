using BankTellerSystem.Server.Contracts;
using BankTellerSystem.Server.Data;
using BankTellerSystem.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankTellerSystem.Server.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController(IDbContextFactory<AppDbContext> dbFactory, AccountTransferService transferService) : ControllerBase
{
    // Teller app: list accounts to pick from when doing a transfer.
    [HttpGet]
    public async Task<ActionResult<List<AccountDto>>> GetAll(CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var accounts = await db.Accounts.OrderBy(a => a.Id).ToListAsync(ct);
        return Ok(accounts.Select(AccountDto.FromDomain).ToList());
    }

    // Teller app: move money from one account to another.
    [HttpPost("transfer")]
    public async Task<ActionResult<TransactionDto>> Transfer([FromBody] TransferRequestDto request, CancellationToken ct)
    {
        try
        {
            var transaction = await transferService.TransferAsync(request.FromAccountId, request.ToAccountId, request.Amount, ct);
            return Ok(TransactionDto.FromDomain(transaction));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}