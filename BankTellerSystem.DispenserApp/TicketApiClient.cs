using System.Net.Http.Json;

namespace BankTellerSystem.DispenserApp;

// Response shape matches the server's TicketDto (Id/Number/Status/etc.).
// We only need a few fields here, so this is a small local record rather
// than sharing the server's DTO project - keeps the dispenser app decoupled.
public record IssuedTicket(int Id, string Number);

// Thin wrapper around the one HTTP call the dispenser terminal needs to make.
// Kept separate from the WinForm so the form has no HTTP/networking code in it.
public class TicketApiClient(HttpClient httpClient)
{
    // Calls POST /api/tickets on the server and returns the newly issued ticket.
    public async Task<IssuedTicket> IssueTicketAsync(CancellationToken ct = default)
    {
        var response = await httpClient.PostAsync("api/tickets", content: null, ct);
        response.EnsureSuccessStatusCode();

        var ticket = await response.Content.ReadFromJsonAsync<IssuedTicket>(cancellationToken: ct);
        return ticket ?? throw new InvalidOperationException("Server returned an empty response.");
    }
}