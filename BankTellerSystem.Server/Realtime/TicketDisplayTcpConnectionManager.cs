using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;

namespace BankTellerSystem.Server.Realtime;

// Keeps track of every ticket-display screen currently connected over TCP,
// and knows how to send a message to all of them at once.
public class TicketDisplayTcpConnectionManager
{
    // One TcpClient = one open phone-call-like connection to one screen.
    // Guid is just a made-up label so we can find/remove a specific one later.
    private readonly ConcurrentDictionary<Guid, TcpClient> _clients = new();

    public Guid Add(TcpClient client)
    {
        var id = Guid.NewGuid();
        _clients[id] = client;
        return id;
    }

    public void Remove(Guid id)
    {
        if (_clients.TryRemove(id, out var client))
            client.Dispose(); // actually closes the connection
    }

    // Writes the same message to every connected screen.
    public async Task BroadcastAsync(string message, CancellationToken ct = default)
    {
        // Our agreed-upon rule: every message ends with a newline, so the
        // reader on the other end knows where it stops.
        var bytes = Encoding.UTF8.GetBytes(message + "\n");

        foreach (var (id, client) in _clients)
        {
            if (!client.Connected)
            {
                Remove(id);
                continue;
            }

            try
            {
                // GetStream() gives us the actual pipe of bytes to write into.
                await client.GetStream().WriteAsync(bytes, ct);
            }
            catch (Exception)
            {
                // That screen's connection died mid-send - drop it, keep going
                // for the others.
                Remove(id);
            }
        }
    }

    public int ConnectionCount => _clients.Count;
}