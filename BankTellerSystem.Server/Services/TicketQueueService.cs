using BankTellerSystem.Domain;
using BankTellerSystem.Server.Data;
using BankTellerSystem.Server.Queueing;
using Microsoft.EntityFrameworkCore;

namespace BankTellerSystem.Server.Services;

// Handles the three ticket-queue operations a teller/dispenser can trigger.
// Every DB-touching operation runs through SerialOperationQueue so concurrent
// requests can never issue duplicate numbers or call the same ticket twice.
public class TicketQueueService(IDbContextFactory<AppDbContext> dbFactory, SerialOperationQueue queue)
{
    // Dispenser terminal: prints and persists the next ticket number.
    public Task<Ticket> IssueTicketAsync(CancellationToken ct = default)
        => queue.Enqueue(async () =>
        {
            await using var db = await dbFactory.CreateDbContextAsync(ct);

            // Simple sequential numbering: A001, A002, ...
            var nextSeq = await db.Tickets.CountAsync(ct) + 1;
            var ticket = new Ticket
            {
                Number = $"A{nextSeq:D3}",
                Status = TicketStatus.Waiting
            };

            db.Tickets.Add(ticket);
            await db.SaveChangesAsync(ct);
            return ticket;
        });

    // Teller: calls the oldest waiting ticket to their counter.
    // Returns null if there's nothing waiting.
    public Task<Ticket?> CallNextAsync(int counterId, CancellationToken ct = default)
        => queue.Enqueue(async () =>
        {
            await using var db = await dbFactory.CreateDbContextAsync(ct);

            var counter = await db.Counters.FindAsync([counterId], ct)
                ?? throw new InvalidOperationException($"Counter {counterId} not found.");

            var next = await db.Tickets
                .Where(t => t.Status == TicketStatus.Waiting)
                .OrderBy(t => t.Id)
                .FirstOrDefaultAsync(ct);

            if (next is null)
                return null;

            next.Status = TicketStatus.Called;
            next.CalledByCounterId = counterId;
            next.CalledAtUtc = DateTime.UtcNow;
            counter.CurrentTicketId = next.Id;

            await db.SaveChangesAsync(ct);
            return next;
        });

    // Teller: marks the counter's current ticket as served and frees the counter.
    public Task<Ticket?> CompleteCurrentAsync(int counterId, CancellationToken ct = default)
        => queue.Enqueue(async () =>
        {
            await using var db = await dbFactory.CreateDbContextAsync(ct);

            var counter = await db.Counters.FindAsync([counterId], ct)
                ?? throw new InvalidOperationException($"Counter {counterId} not found.");

            if (counter.CurrentTicketId is null)
                return null;

            var ticket = await db.Tickets.FindAsync([counter.CurrentTicketId.Value], ct);
            if (ticket is not null)
                ticket.Status = TicketStatus.Served;

            counter.CurrentTicketId = null;

            await db.SaveChangesAsync(ct);
            return ticket;
        });
}