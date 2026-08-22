using System.Net;
using System.Net.Http.Json;
using BankTellerSystem.Server.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BankTellerSystem.Tests.Server.Controllers;

[TestClass]
public class AccountsControllerTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private string _dbPath = null!;

    [TestInitialize]
    public void Setup()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"bankteller_it_{Guid.NewGuid():N}.db");

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Default", $"Data Source={_dbPath}");
            builder.UseSetting("TicketDisplayTcp:Port", "0"); // let the OS pick a free port
        });

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

    [TestMethod]
    public async Task GetAll_ReturnsSeededAccounts()
    {
        var response = await _client.GetAsync("/api/accounts");
        response.EnsureSuccessStatusCode();

        var accounts = await response.Content.ReadFromJsonAsync<List<AccountDto>>();

        Assert.IsNotNull(accounts);
        Assert.AreEqual(2, accounts.Count);
    }

    [TestMethod]
    public async Task Transfer_ValidRequest_MovesBalance()
    {
        var request = new TransferRequestDto(FromAccountId: 1, ToAccountId: 2, Amount: 100_000m);

        var response = await _client.PostAsJsonAsync("/api/accounts/transfer", request);
        response.EnsureSuccessStatusCode();

        var transaction = await response.Content.ReadFromJsonAsync<TransactionDto>();
        Assert.IsNotNull(transaction);
        Assert.AreEqual(100_000m, transaction.Amount);

        var accountsResponse = await _client.GetAsync("/api/accounts");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<List<AccountDto>>();
        var from = accounts!.Single(a => a.Id == 1);
        Assert.AreEqual(900_000m, from.Balance);
    }

    [TestMethod]
    public async Task Transfer_InsufficientFunds_ReturnsBadRequest()
    {
        var request = new TransferRequestDto(FromAccountId: 1, ToAccountId: 2, Amount: 999_999_999m);

        var response = await _client.PostAsJsonAsync("/api/accounts/transfer", request);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
}