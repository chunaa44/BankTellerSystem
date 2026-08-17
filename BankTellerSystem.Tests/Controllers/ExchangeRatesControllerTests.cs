using System.Net;
using System.Net.Http.Json;
using BankTellerSystem.Server.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BankTellerSystem.Tests.Controllers;

[TestClass]
public class ExchangeRatesControllerTests
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
    public async Task GetAll_ReturnsSeededRates()
    {
        var response = await _client.GetAsync("/api/exchange-rates");
        response.EnsureSuccessStatusCode();

        var rates = await response.Content.ReadFromJsonAsync<List<ExchangeRateDto>>();

        Assert.IsNotNull(rates);
        Assert.AreEqual(3, rates.Count);
    }

    [TestMethod]
    public async Task UpdateRate_ValidRequest_UpdatesRate()
    {
        var request = new UpdateExchangeRateRequestDto("USD", BuyRate: 3500m, SellRate: 3520m);

        var response = await _client.PutAsJsonAsync("/api/exchange-rates/USD", request);
        response.EnsureSuccessStatusCode();

        var rate = await response.Content.ReadFromJsonAsync<ExchangeRateDto>();
        Assert.IsNotNull(rate);
        Assert.AreEqual(3500m, rate.BuyRate);
    }

    [TestMethod]
    public async Task UpdateRate_MismatchedRouteAndBodyCode_ReturnsBadRequest()
    {
        var request = new UpdateExchangeRateRequestDto("EUR", BuyRate: 3500m, SellRate: 3520m);

        var response = await _client.PutAsJsonAsync("/api/exchange-rates/USD", request);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task UpdateRate_UnknownCurrency_ReturnsBadRequest()
    {
        var request = new UpdateExchangeRateRequestDto("GBP", BuyRate: 100m, SellRate: 110m);

        var response = await _client.PutAsJsonAsync("/api/exchange-rates/GBP", request);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
}