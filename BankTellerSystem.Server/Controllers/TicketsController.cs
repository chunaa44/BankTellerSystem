using BankTellerSystem.Server.Contracts;
using BankTellerSystem.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankTellerSystem.Server.Controllers;

[ApiController]
[Route("api/tickets")]
public class TicketsController(TicketQueueService ticketQueueService) : ControllerBase
{
    // Dispenser terminal calls this to print the next ticket.
    [HttpPost]
    public async Task<ActionResult<TicketDto>> IssueTicket(CancellationToken ct)
    {
        var ticket = await ticketQueueService.IssueTicketAsync(ct);
        return Ok(TicketDto.FromDomain(ticket));
    }

    // Teller app calls this to call the next waiting customer to a counter.
    [HttpPost("call-next")]
    public async Task<ActionResult<TicketDto>> CallNext([FromQuery] int counterId, CancellationToken ct)
    {
        try
        {
            var ticket = await ticketQueueService.CallNextAsync(counterId, ct);
            return ticket is null ? NoContent() : Ok(TicketDto.FromDomain(ticket));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Teller app calls this once they've finished serving their current ticket.
    [HttpPost("complete-current")]
    public async Task<ActionResult<TicketDto>> CompleteCurrent([FromQuery] int counterId, CancellationToken ct)
    {
        try
        {
            var ticket = await ticketQueueService.CompleteCurrentAsync(counterId, ct);
            return ticket is null ? NoContent() : Ok(TicketDto.FromDomain(ticket));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}