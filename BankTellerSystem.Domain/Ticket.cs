namespace BankTellerSystem.Domain;

// A queue number printed by the dispenser terminal at the bank entrance.
public class Ticket
{
    public int Id { get; set; }

    // The number shown on the printed slip and on the display screen, e.g. "A014".
    public string Number { get; set; } = string.Empty;

    public TicketStatus Status { get; set; } = TicketStatus.Waiting;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Set when a teller calls this ticket.
    public int? CalledByCounterId { get; set; }

    public DateTime? CalledAtUtc { get; set; }
}

public enum TicketStatus
{
    Waiting,
    Called,
    Served
}