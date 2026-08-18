using System.Net;
using System.Net.Sockets;

namespace BankTellerSystem.Server.Realtime;

// Runs for the entire lifetime of the app: listens on a TCP port, and every
// time a ticket-display screen connects, hands that connection to
// TicketDisplayTcpConnectionManager so it can receive broadcasts.
public class TicketDisplayTcpServer(TicketDisplayTcpConnectionManager connections, IConfiguration config)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var port = config.GetValue("TicketDisplayTcp:Port", 5050);

        // "Answer the phone": claim this port and start listening on it.
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        try
        {
            // Keep accepting new callers for as long as the app is running.
            while (!stoppingToken.IsCancellationRequested)
            {
                var client = await listener.AcceptTcpClientAsync(stoppingToken);
                var id = connections.Add(client);

                // Watch this one connection for a disconnect, without
                // blocking this loop from accepting the NEXT screen.
                _ = MonitorConnectionAsync(client, id, stoppingToken);
            }
        }
        finally
        {
            listener.Stop();
        }
    }

    // Ticket displays never send us anything meaningful - they only receive.
    // The only way to find out is to keep trying to read: a read that comes
    // back with 0 bytes means the other side closed the connection cleanly.
    private async Task MonitorConnectionAsync(TcpClient client, Guid id, CancellationToken ct)
    {
        var buffer = new byte[256];
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var bytesRead = await client.GetStream().ReadAsync(buffer, ct);
                if (bytesRead == 0) break; // screen disconnected
            }
        }
        catch (Exception)
        {
            // Connection dropped abruptly (network cut, app crashed, etc.) -
            // treat it the same as a clean disconnect.
        }
        finally
        {
            connections.Remove(id);
        }
    }
}