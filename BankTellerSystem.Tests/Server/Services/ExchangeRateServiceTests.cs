using BankTellerSystem.Server.Data;
using BankTellerSystem.Server.Queueing;
using BankTellerSystem.Server.Services;
using Microsoft.EntityFrameworkCore;

namespace BankTellerSystem.Tests.Server.Services;

[TestClass]
public class ExchangeRateServiceTests
{
    private string _dbPath = null!;
    private ExchangeRateService _service = null!;

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
            db.Database.EnsureCreated(); // applies schema + HasData seed (USD, EUR, CNY)
        }

        _service = new ExchangeRateService(new TestDbContextFactory(options), new SerialOperationQueue());
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

    // Minimal test-only factory: same DbContextOptions, new context per call.
    private sealed class TestDbContextFactory(DbContextOptions<AppDbContext> options) : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext() => new(options);
    }
}