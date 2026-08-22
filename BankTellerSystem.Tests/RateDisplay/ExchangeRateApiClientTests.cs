extern alias RateDisplayAssembly;

using System.Net;
using RateDisplayAssembly::BankTellerSystem.RateDisplay.Services;

namespace BankTellerSystem.Tests.RateDisplay;

[TestClass]
public class ExchangeRateApiClientTests
{
    [TestMethod]
    public async Task GetAllAsync_ReturnsRatesFromServerResponse()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK,
            """[{"currencyCode":"USD","buyRate":3450,"sellRate":3470,"updatedAtUtc":"2026-01-01T00:00:00Z"}]""");
        var client = new ExchangeRateApiClient(new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") });

        var rates = await client.GetAllAsync();

        Assert.AreEqual(1, rates.Count);
        Assert.AreEqual("USD", rates[0].CurrencyCode);
    }

    [TestMethod]
    public async Task GetAllAsync_RequestsExchangeRatesEndpoint()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "[]");
        var client = new ExchangeRateApiClient(new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") });

        await client.GetAllAsync();

        Assert.AreEqual(HttpMethod.Get, handler.LastRequest!.Method);
        Assert.AreEqual("http://localhost/api/exchange-rates", handler.LastRequest.RequestUri!.ToString());
    }

    // Minimal fake handler
    private sealed class FakeHttpMessageHandler(HttpStatusCode statusCode, string responseBody) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(statusCode) { Content = new StringContent(responseBody) });
        }
    }
}