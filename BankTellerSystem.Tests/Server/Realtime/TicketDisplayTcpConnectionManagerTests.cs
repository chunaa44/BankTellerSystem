using System.Net.Sockets;
using BankTellerSystem.Server.Realtime;
using BankTellerSystem.Tests.Server.TestSupport;

namespace BankTellerSystem.Tests.Server.Realtime;

[TestClass]
public class TicketDisplayTcpConnectionManagerTests
{
    [TestMethod]
    public async Task Add_ReturnsUniqueId_AndIncreasesConnectionCount()
    {
        var manager = new TicketDisplayTcpConnectionManager();
        await using var pair = await LoopbackTcpPair.CreateAsync();

        var id = manager.Add(pair.ServerSideClient);

        Assert.AreNotEqual(Guid.Empty, id);
        Assert.AreEqual(1, manager.ConnectionCount);
    }

    [TestMethod]
    public async Task Add_CalledTwice_ReturnsDifferentIds()
    {
        var manager = new TicketDisplayTcpConnectionManager();
        await using var pairA = await LoopbackTcpPair.CreateAsync();
        await using var pairB = await LoopbackTcpPair.CreateAsync();

        var idA = manager.Add(pairA.ServerSideClient);
        var idB = manager.Add(pairB.ServerSideClient);

        Assert.AreNotEqual(idA, idB);
        Assert.AreEqual(2, manager.ConnectionCount);
    }

    [TestMethod]
    public async Task Remove_ClosesConnection_AndDecreasesConnectionCount()
    {
        var manager = new TicketDisplayTcpConnectionManager();
        await using var pair = await LoopbackTcpPair.CreateAsync();
        var id = manager.Add(pair.ServerSideClient);

        manager.Remove(id);

        Assert.AreEqual(0, manager.ConnectionCount);
        Assert.IsFalse(pair.ServerSideClient.Connected);
    }

    [TestMethod]
    public void Remove_UnknownId_DoesNothing()
    {
        // Removing an id that was never added (e.g. a double-remove race)
        // must not throw.
        var manager = new TicketDisplayTcpConnectionManager();

        manager.Remove(Guid.NewGuid());

        Assert.AreEqual(0, manager.ConnectionCount);
    }

    [TestMethod]
    public async Task BroadcastAsync_SendsMessageToConnectedScreen()
    {
        var manager = new TicketDisplayTcpConnectionManager();
        await using var pair = await LoopbackTcpPair.CreateAsync();
        manager.Add(pair.ServerSideClient);

        await manager.BroadcastAsync("A001");

        Assert.AreEqual("A001", await pair.ReadLineFromScreenAsync());
    }

    [TestMethod]
    public async Task BroadcastAsync_SendsMessageToEveryConnectedScreen()
    {
        var manager = new TicketDisplayTcpConnectionManager();
        await using var pairA = await LoopbackTcpPair.CreateAsync();
        await using var pairB = await LoopbackTcpPair.CreateAsync();
        manager.Add(pairA.ServerSideClient);
        manager.Add(pairB.ServerSideClient);

        await manager.BroadcastAsync("A002");

        Assert.AreEqual("A002", await pairA.ReadLineFromScreenAsync());
        Assert.AreEqual("A002", await pairB.ReadLineFromScreenAsync());
    }

    [TestMethod]
    public async Task BroadcastAsync_RemovesClientThatIsNotConnected()
    {
        var manager = new TicketDisplayTcpConnectionManager();
        // A TcpClient that was never connected reports Connected == false -
        // the same check BroadcastAsync uses to spot a dead screen.
        using var neverConnected = new TcpClient();
        var id = manager.Add(neverConnected);

        await manager.BroadcastAsync("A003");

        Assert.AreEqual(0, manager.ConnectionCount);
    }

    [TestMethod]
    public async Task BroadcastAsync_WithNoConnections_DoesNothing()
    {
        var manager = new TicketDisplayTcpConnectionManager();

        await manager.BroadcastAsync("A004");

        Assert.AreEqual(0, manager.ConnectionCount);
    }
}