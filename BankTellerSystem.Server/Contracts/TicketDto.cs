using BankTellerSystem.Domain;

namespace BankTellerSystem.Server.Contracts;

// Response shape for ticket queue operations (issue/call/complete).
public record TicketDto(
    int Id,
    string Number,
    TicketStatus Status,
    DateTime CreatedAtUtc,
    int? CalledByCounterId,
    DateTime? CalledAtUtc)
{
    public static TicketDto FromDomain(Ticket ticket) => new(
        ticket.Id,
        ticket.Number,
        ticket.Status,
        ticket.CreatedAtUtc,
        ticket.CalledByCounterId,
        ticket.CalledAtUtc);
}