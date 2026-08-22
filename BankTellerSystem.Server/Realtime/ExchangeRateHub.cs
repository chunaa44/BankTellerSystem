using Microsoft.AspNetCore.SignalR;

namespace BankTellerSystem.Server.Realtime;

// Push-only hub for the Blazor rate-display screen: it connects and just
// listens for "RateUpdated" messages - it never calls anything on this hub,
// so there's nothing to implement here.
public class ExchangeRateHub : Hub
{
}