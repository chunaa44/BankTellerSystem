using BankTellerSystem.Domain;
using BankTellerSystem.Server.Data;
using BankTellerSystem.Server.Queueing;
using BankTellerSystem.Server.Services;
using Microsoft.EntityFrameworkCore;

namespace BankTellerSystem.Tests.Services;

[TestClass]
public class AccountTransferServiceTests
{
    private string _dbPath = null!;
    private AppDbContext _verifyDb = null!;
    private AccountTransferService _service = null!;

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
            db.Database.EnsureCreated(); // applies schema + HasData seed (Accounts 1, 2)
        }

        _verifyDb = new AppDbContext(options);
        _service = new AccountTransferService(new TestDbContextFactory(options), new SerialOperationQueue());
    }

    [TestCleanup]
    public void Cleanup()
    {
        _verifyDb.Dispose();

        // Microsoft.Data.Sqlite pools connections by connection string, so the
        // file handle can outlive the DbContext that used it. Clear pools first.
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

        if (File.Exists(_dbPath)) File.Delete(_dbPath);
    }

    [TestMethod]
    public async Task TransferAsync_MovesBalanceBetweenAccounts()
    {
        // Seed: Account 1 = 1,000,000, Account 2 = 500,000.
        await _service.TransferAsync(fromAccountId: 1, toAccountId: 2, amount: 100_000m);

        var from = await _verifyDb.Accounts.FindAsync(1);
        var to = await _verifyDb.Accounts.FindAsync(2);

        Assert.AreEqual(900_000m, from!.Balance);
        Assert.AreEqual(600_000m, to!.Balance);
    }

    [TestMethod]
    public async Task TransferAsync_RecordsTransaction()
    {
        var transaction = await _service.TransferAsync(fromAccountId: 1, toAccountId: 2, amount: 50_000m);

        Assert.AreEqual(1, transaction.FromAccountId);
        Assert.AreEqual(2, transaction.ToAccountId);
        Assert.AreEqual(50_000m, transaction.Amount);
    }

    [TestMethod]
    public async Task TransferAsync_SameAccount_Throws()
    {
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => _service.TransferAsync(fromAccountId: 1, toAccountId: 1, amount: 100m));
    }

    [TestMethod]
    public async Task TransferAsync_NonPositiveAmount_Throws()
    {
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => _service.TransferAsync(fromAccountId: 1, toAccountId: 2, amount: 0m));
    }

    [TestMethod]
    public async Task TransferAsync_InsufficientFunds_Throws()
    {
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => _service.TransferAsync(fromAccountId: 2, toAccountId: 1, amount: 999_999_999m));
    }

    [TestMethod]
    public async Task TransferAsync_UnknownFromAccount_Throws()
    {
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => _service.TransferAsync(fromAccountId: 999, toAccountId: 1, amount: 100m));
    }

    [TestMethod]
    public async Task TransferAsync_UnknownToAccount_Throws()
    {
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => _service.TransferAsync(fromAccountId: 1, toAccountId: 999, amount: 100m));
    }

    [TestMethod]
    public async Task TransferAsync_ConcurrentTransfersFromSameAccount_NeverOverdraws()
    {
        // Account 1 starts at 1,000,000. Fire 15 concurrent transfers of 100,000
        // each (1,500,000 total demand) - the queue must serialize them so the
        // balance never goes negative and exactly 10 succeed.
        var tasks = Enumerable.Range(0, 15)
            .Select(_ => _service.TransferAsync(fromAccountId: 1, toAccountId: 2, amount: 100_000m))
            .ToArray();

        var results = await Task.WhenAll(tasks.Select(async t =>
        {
            try { await t; return true; }
            catch (InvalidOperationException) { return false; }
        }));

        var succeeded = results.Count(r => r);
        Assert.AreEqual(10, succeeded);

        var from = await _verifyDb.Accounts.FindAsync(1);
        Assert.AreEqual(0m, from!.Balance);
    }

    // Minimal test-only factory: same DbContextOptions, new context per call.
    private sealed class TestDbContextFactory(DbContextOptions<AppDbContext> options) : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext() => new(options);
    }
}