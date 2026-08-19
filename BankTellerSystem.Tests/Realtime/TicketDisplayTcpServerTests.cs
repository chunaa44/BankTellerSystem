using System.Net;
using System.Net.Sockets;
using BankTellerSystem.Server.Realtime;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BankTellerSystem.Tests.Realtime;

[TestClass]
public class TicketDisplayTcpServerTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private string _dbPath = null!;
    private int _tcpPort;

    [TestInitialize]
    public void Setup()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"bankteller_it_{Guid.NewGuid():N}.db");
        _tcpPort = GetFreeTcpPort();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Default", $"Data Source={_dbPath}");
            builder.UseSetting("TicketDisplayTcp:Port", _tcpPort.ToString());
        });

        // Creating a client is what actually starts the host (and therefore
        // the TicketDisplayTcpServer background service) under the hood.
        _client = _factory.CreateClient();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client.Dispose();
        _factory.Dispose();

        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        if (File.Exists(_dbPath)) File.Delete(_dbPath);
    }

    // Grabs a port nobody's listening on yet by briefly binding to port 0
    // (the OS picks a free one) and immediately releasing it.
    private static int GetFreeTcpPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    // The accept loop and the disconnect-monitor loop both run on
    // background tasks, so tests have to poll for their effects instead of
    // seeing them immediately.
    private static async Task WaitUntilAsync(Func<bool> condition, string failureMessage)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (DateTime.UtcNow < deadline)
        {
            if (condition()) return;
            await Task.Delay(25);
        }

        Assert.Fail(failureMessage);
    }

    private TicketDisplayTcpConnectionManager Connections =>
        _factory.Services.GetRequiredService<TicketDisplayTcpConnectionManager>();

    [TestMethod]
    public async Task ConnectingScreen_IsTrackedByConnectionManager()
    {
        using var screen = new TcpClient();
        await screen.ConnectAsync(IPAddress.Loopback, _tcpPort);

        await WaitUntilAsync(() => Connections.ConnectionCount == 1,
            "Expected the server to accept the connection and register it.");
    }

    [TestMethod]
    public async Task BroadcastFromConnectionManager_ReachesConnectedScreen()
    {
        using var screen = new TcpClient();
        await screen.ConnectAsync(IPAddress.Loopback, _tcpPort);
        await WaitUntilAsync(() => Connections.ConnectionCount == 1,
            "Expected the server to accept the connection.");

        await Connections.BroadcastAsync("A010");

        using var reader = new StreamReader(screen.GetStream());
        var line = await reader.ReadLineAsync();
        Assert.AreEqual("A010", line);
    }

    [TestMethod]
    public async Task DisconnectingScreen_IsRemovedFromConnectionManager()
    {
        var screen = new TcpClient();
        await screen.ConnectAsync(IPAddress.Loopback, _tcpPort);
        await WaitUntilAsync(() => Connections.ConnectionCount == 1,
            "Expected the server to accept the connection.");

        // A clean close on the screen's side should be picked up by
        // TicketDisplayTcpServer's monitor loop (a 0-byte read).
        screen.Dispose();

        await WaitUntilAsync(() => Connections.ConnectionCount == 0,
            "Expected the server to notice the disconnect and remove the screen.");
    }

    [TestMethod]
    public async Task MultipleScreens_AllGetAddedAndAllReceiveBroadcast()
    {
        using var screenA = new TcpClient();
        using var screenB = new TcpClient();
        await screenA.ConnectAsync(IPAddress.Loopback, _tcpPort);
        await screenB.ConnectAsync(IPAddress.Loopback, _tcpPort);

        await WaitUntilAsync(() => Connections.ConnectionCount == 2,
            "Expected the server to accept both connections.");

        await Connections.BroadcastAsync("A011");

        using var readerA = new StreamReader(screenA.GetStream());
        using var readerB = new StreamReader(screenB.GetStream());
        Assert.AreEqual("A011", await readerA.ReadLineAsync());
        Assert.AreEqual("A011", await readerB.ReadLineAsync());
    }
}