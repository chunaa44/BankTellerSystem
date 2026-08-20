using System.Net;
using System.Net.Sockets;

namespace BankTellerSystem.Tests.Server.TestSupport;

// A real, connected pair of TcpClients on the loopback adapter: one end
// plays the role of "the server" (what TicketDisplayTcpConnectionManager
// would hold), the other plays "the screen" (what we read from / disconnect
// in tests). We use real sockets instead of mocks because TcpClient and
// NetworkStream are not designed to be mocked.
internal sealed class LoopbackTcpPair : IAsyncDisposable
{
    private readonly TcpListener _listener;

    public TcpClient ServerSideClient { get; }
    public TcpClient ScreenSideClient { get; }

    private LoopbackTcpPair(TcpListener listener, TcpClient serverSide, TcpClient screenSide)
    {
        _listener = listener;
        ServerSideClient = serverSide;
        ScreenSideClient = screenSide;
    }

    // Opens a listener on a free loopback port, connects to it, and hands
    // back both ends of the resulting connection.
    public static async Task<LoopbackTcpPair> CreateAsync()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        var screenSide = new TcpClient();
        var connectTask = screenSide.ConnectAsync(IPAddress.Loopback, port);
        var serverSide = await listener.AcceptTcpClientAsync();
        await connectTask;

        return new LoopbackTcpPair(listener, serverSide, screenSide);
    }

    // Reads one newline-terminated message from the screen's side of the
    // connection - i.e. what a real display screen would receive.
    public async Task<string?> ReadLineFromScreenAsync()
    {
        using var reader = new StreamReader(ScreenSideClient.GetStream(), leaveOpen: true);
        return await reader.ReadLineAsync();
    }

    public async ValueTask DisposeAsync()
    {
        ScreenSideClient.Dispose();
        ServerSideClient.Dispose();
        _listener.Stop();
        await Task.CompletedTask;
    }
}