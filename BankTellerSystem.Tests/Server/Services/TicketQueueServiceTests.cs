using BankTellerSystem.Domain;
using BankTellerSystem.Server.Data;
using BankTellerSystem.Server.Queueing;
using BankTellerSystem.Server.Realtime;
using BankTellerSystem.Server.Services;
using Microsoft.EntityFrameworkCore;

namespace BankTellerSystem.Tests.Server.Services;

[TestClass]
public class TicketQueueServiceTests
{
    private string _dbPath = null!;
    private TicketQueueService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        // Fresh SQLite file per test so tests don't interfere with each other.
        _dbPath = Path.Combine(Path.GetTempPath(), $"bankteller_test_{Guid.NewGuid():N}.db");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={_dbPath}")
            .Options;

        using (var db = new AppDbContext(options))
        {
            db.Database.EnsureCreated(); // applies schema + HasData seed (Counters 1-3)
        }

        _service = new TicketQueueService(
            new TestDbContextFactory(options),
            new SerialOperationQueue(),
            new TicketDisplayTcpConnectionManager());
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Microsoft.Data.Sqlite pools connections by connection string, so the
        // file handle can outlive the DbContext that used it. Clear pools first.
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

        if (File.Exists(_dbPath)) File.Delete(_dbPath);
    }

    [TestMethod]
    public async Task IssueTicketAsync_FirstTicket_ReturnsA001()
    {
        var ticket = await _service.IssueTicketAsync();

        Assert.AreEqual("A001", ticket.Number);
        Assert.AreEqual(TicketStatus.Waiting, ticket.Status);
    }

    [TestMethod]
    public async Task IssueTicketAsync_MultipleCalls_IncrementSequentially()
    {
        var t1 = await _service.IssueTicketAsync();
        var t2 = await _service.IssueTicketAsync();
        var t3 = await _service.IssueTicketAsync();

        Assert.AreEqual("A001", t1.Number);
        Assert.AreEqual("A002", t2.Number);
        Assert.AreEqual("A003", t3.Number);
    }

    [TestMethod]
    public async Task CallNextAsync_NoWaitingTickets_ReturnsNull()
    {
        var result = await _service.CallNextAsync(counterId: 1);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task CallNextAsync_CallsOldestWaitingTicketFirst()
    {
        var first = await _service.IssueTicketAsync();
        await _service.IssueTicketAsync();

        var called = await _service.CallNextAsync(counterId: 1);

        Assert.IsNotNull(called);
        Assert.AreEqual(first.Id, called!.Id);
        Assert.AreEqual(TicketStatus.Called, called.Status);
        Assert.AreEqual(1, called.CalledByCounterId);
    }

    [TestMethod]
    public async Task CallNextAsync_UnknownCounter_Throws()
    {
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => _service.CallNextAsync(counterId: 999));
    }

    [TestMethod]
    public async Task CompleteCurrentAsync_MarksServedAndFreesCounter()
    {
        await _service.IssueTicketAsync();
        await _service.CallNextAsync(counterId: 1);

        var completed = await _service.CompleteCurrentAsync(counterId: 1);

        Assert.IsNotNull(completed);
        Assert.AreEqual(TicketStatus.Served, completed!.Status);

        // Counter is free again -> nothing to call, but queue is also empty.
        var next = await _service.CallNextAsync(counterId: 1);
        Assert.IsNull(next);
    }

    [TestMethod]
    public async Task CompleteCurrentAsync_NoCurrentTicket_ReturnsNull()
    {
        var result = await _service.CompleteCurrentAsync(counterId: 2);

        Assert.IsNull(result);
    }

    // Minimal test-only factory: same DbContextOptions, new context per call.
    private sealed class TestDbContextFactory(DbContextOptions<AppDbContext> options) : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext() => new(options);
    }
}