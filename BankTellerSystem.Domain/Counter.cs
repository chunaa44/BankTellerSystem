namespace BankTellerSystem.Domain;

// A teller work station (e.g. "Counter 3"). Each counter can have at most
// one ticket "currently called" at a time - this is what stops the same
// number from being shown on more than one display simultaneously.
public class Counter
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    // The ticket this counter is currently serving, if any.
    public int? CurrentTicketId { get; set; }
}