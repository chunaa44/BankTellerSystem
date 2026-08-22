using BankTellerSystem.Server.Data;
using BankTellerSystem.Server.Queueing;
using BankTellerSystem.Server.Realtime;
using BankTellerSystem.Server.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BankTellerSystem.Tests.Server.Services;

[TestClass]
public class ExchangeRateServiceTests
{
    private string _dbPath = null!;
    private ExchangeRateService _service = null!;
    private FakeHubContext _hub = null!;

    [TestInitialize]
    public void Setup()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"bankteller_test_{Guid.NewGuid():N}.db");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={_dbPath}")
            .Options;

        using (var db = new AppDbContext(options))
        {
            db.Database.EnsureCreated();
        }

        _hub = new FakeHubContext();
        _service = new ExchangeRateService(new TestDbContextFactory(options), new SerialOperationQueue(), _hub);
    }

    [TestCleanup]
    public void Cleanup()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        if (File.Exists(_dbPath)) File.Delete(_dbPath);
    }

    [TestMethod]
    public async Task GetAllAsync_ReturnsSeededRatesOrderedByCode()
    {
        var rates = await _service.GetAllAsync();

        Assert.AreEqual(3, rates.Count);
        CollectionAssert.AreEqual(
            new[] { "CNY", "EUR", "USD" },
            rates.Select(r => r.CurrencyCode).ToArray());
    }

    [TestMethod]
    public async Task UpdateRateAsync_UpdatesBuyAndSellRate()
    {
        var updated = await _service.UpdateRateAsync("USD", buyRate: 3500m, sellRate: 3520m);

        Assert.AreEqual(3500m, updated.BuyRate);
        Assert.AreEqual(3520m, updated.SellRate);

        var rates = await _service.GetAllAsync();
        var usd = rates.Single(r => r.CurrencyCode == "USD");
        Assert.AreEqual(3500m, usd.BuyRate);
        Assert.AreEqual(3520m, usd.SellRate);
    }

    [TestMethod]
    public async Task UpdateRateAsync_UnknownCurrency_Throws()
    {
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => _service.UpdateRateAsync("GBP", buyRate: 100m, sellRate: 110m));
    }

    [TestMethod]
    public async Task UpdateRateAsync_NonPositiveRate_Throws()
    {
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => _service.UpdateRateAsync("USD", buyRate: 0m, sellRate: 100m));
    }

    [TestMethod]
    public async Task UpdateRateAsync_BuyExceedsSell_Throws()
    {
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => _service.UpdateRateAsync("USD", buyRate: 3600m, sellRate: 3500m));
    }

    [TestMethod]
    public async Task UpdateRateAsync_BroadcastsRateUpdatedToAllClients()
    {
        await _service.UpdateRateAsync("USD", buyRate: 3500m, sellRate: 3520m);

        var call = _hub.AllClients.Calls.Single();
        Assert.AreEqual("RateUpdated", call.Method);
    }

    [TestMethod]
    public async Task UpdateRateAsync_Throws_DoesNotBroadcast()
    {
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => _service.UpdateRateAsync("GBP", buyRate: 100m, sellRate: 110m));

        Assert.AreEqual(0, _hub.AllClients.Calls.Count);
    }

    // Minimal test-only factory: same DbContextOptions, new context per call.
    private sealed class TestDbContextFactory(DbContextOptions<AppDbContext> options) : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext() => new(options);
    }
}

// Minimal stand-in for IHubContext<ExchangeRateHub>. Only "Clients.All" is
// ever used by ExchangeRateService, so everything else throws if touched -
// that would mean the service started relying on something this fake
// doesn't support yet.
internal sealed class FakeHubContext : IHubContext<ExchangeRateHub>
{
    public FakeClientProxy AllClients { get; } = new();
    public IHubClients Clients => new FakeHubClients(AllClients);
    public IGroupManager Groups => throw new NotSupportedException("Not used by ExchangeRateService.");

    private sealed class FakeHubClients(FakeClientProxy all) : IHubClients
    {
        public IClientProxy All => all;
        public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => throw new NotSupportedException();
        public IClientProxy Client(string connectionId) => throw new NotSupportedException();
        public IClientProxy Clients(IReadOnlyList<string> connectionIds) => throw new NotSupportedException();
        public IClientProxy Group(string groupName) => throw new NotSupportedException();
        public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => throw new NotSupportedException();
        public IClientProxy Groups(IReadOnlyList<string> groupNames) => throw new NotSupportedException();
        public IClientProxy OthersInGroup(string groupName) => throw new NotSupportedException();
        public IClientProxy User(string userId) => throw new NotSupportedException();
        public IClientProxy Users(IReadOnlyList<string> userIds) => throw new NotSupportedException();
    }
}

// Records every SendAsync call ("RateUpdated", the payload) so tests can
// assert a broadcast happened, without needing a real SignalR connection.
internal sealed class FakeClientProxy : IClientProxy
{
    public List<(string Method, object?[] Args)> Calls { get; } = [];

    public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = default)
    {
        Calls.Add((method, args));
        return Task.CompletedTask;
    }
}