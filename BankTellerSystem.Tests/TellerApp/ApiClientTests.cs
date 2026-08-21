using System.Net;
using System.Net.Http.Json;
using BankTellerSystem.TellerApp;

namespace BankTellerSystem.Tests.TellerApp;

[TestClass]
public class ApiClientTests
{
    [TestMethod]
    public async Task GetAccountsAsync_ReturnsAccountsFromServerResponse()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK,
            """[{"id":1,"accountNumber":"1000000001","ownerName":"Bat","balance":1000000}]""");
        var client = CreateClient(handler);

        var accounts = await client.GetAccountsAsync();

        Assert.AreEqual(1, accounts.Count);
        Assert.AreEqual("Bat", accounts[0].OwnerName);
    }

    [TestMethod]
    public async Task GetAccountsAsync_RequestsAccountsEndpoint()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "[]");
        var client = CreateClient(handler);

        await client.GetAccountsAsync();

        Assert.AreEqual(HttpMethod.Get, handler.LastRequest!.Method);
        Assert.AreEqual("http://localhost/api/accounts", handler.LastRequest.RequestUri!.ToString());
    }

    [TestMethod]
    public async Task GetExchangeRatesAsync_ReturnsRatesFromServerResponse()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK,
            """[{"id":1,"currencyCode":"USD","buyRate":3450,"sellRate":3470,"updatedAtUtc":"2026-01-01T00:00:00Z"}]""");
        var client = CreateClient(handler);

        var rates = await client.GetExchangeRatesAsync();

        Assert.AreEqual(1, rates.Count);
        Assert.AreEqual("USD", rates[0].CurrencyCode);
    }

    [TestMethod]
    public async Task CallNextAsync_WithWaitingTicket_ReturnsTicket()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK,
            """{"id":1,"number":"A001","status":1,"createdAtUtc":"2026-01-01T00:00:00Z","calledByCounterId":1,"calledAtUtc":"2026-01-01T00:00:00Z"}""");
        var client = CreateClient(handler);

        var ticket = await client.CallNextAsync(counterId: 1);

        Assert.IsNotNull(ticket);
        Assert.AreEqual("A001", ticket.Number);
        Assert.AreEqual("http://localhost/api/tickets/call-next?counterId=1", handler.LastRequest!.RequestUri!.ToString());
    }

    [TestMethod]
    public async Task CallNextAsync_NoWaitingTickets_ReturnsNull()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.NoContent, "");
        var client = CreateClient(handler);

        var ticket = await client.CallNextAsync(counterId: 1);

        Assert.IsNull(ticket);
    }

    [TestMethod]
    public async Task CallNextAsync_ServerError_ThrowsWithServerMessage()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.BadRequest, "Counter 999 not found.");
        var client = CreateClient(handler);

        var ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => client.CallNextAsync(counterId: 999));
        Assert.AreEqual("Counter 999 not found.", ex.Message);
    }

    [TestMethod]
    public async Task CompleteCurrentAsync_WithTicketInProgress_ReturnsTicket()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK,
            """{"id":1,"number":"A001","status":2,"createdAtUtc":"2026-01-01T00:00:00Z","calledByCounterId":1,"calledAtUtc":"2026-01-01T00:00:00Z"}""");
        var client = CreateClient(handler);

        var ticket = await client.CompleteCurrentAsync(counterId: 1);

        Assert.IsNotNull(ticket);
        Assert.AreEqual("A001", ticket.Number);
    }

    [TestMethod]
    public async Task CompleteCurrentAsync_NothingInProgress_ReturnsNull()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.NoContent, "");
        var client = CreateClient(handler);

        var ticket = await client.CompleteCurrentAsync(counterId: 1);

        Assert.IsNull(ticket);
    }

    [TestMethod]
    public async Task TransferAsync_PostsToTransferEndpoint_ReturnsTransaction()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK,
            """{"id":1,"fromAccountId":1,"toAccountId":2,"amount":100000,"createdAtUtc":"2026-01-01T00:00:00Z"}""");
        var client = CreateClient(handler);

        var transaction = await client.TransferAsync(fromAccountId: 1, toAccountId: 2, amount: 100_000m);

        Assert.AreEqual(100_000m, transaction.Amount);
        Assert.AreEqual(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.AreEqual("http://localhost/api/accounts/transfer", handler.LastRequest.RequestUri!.ToString());
    }

    [TestMethod]
    public async Task TransferAsync_ServerError_ThrowsWithServerMessage()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.BadRequest, "Insufficient funds.");
        var client = CreateClient(handler);

        var ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => client.TransferAsync(fromAccountId: 1, toAccountId: 2, amount: 999_999_999m));
        Assert.AreEqual("Insufficient funds.", ex.Message);
    }

    [TestMethod]
    public async Task UpdateRateAsync_PutsToCorrectEndpoint_ReturnsUpdatedRate()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK,
            """{"id":1,"currencyCode":"USD","buyRate":3500,"sellRate":3520,"updatedAtUtc":"2026-01-01T00:00:00Z"}""");
        var client = CreateClient(handler);

        var updated = await client.UpdateRateAsync("USD", buyRate: 3500m, sellRate: 3520m);

        Assert.AreEqual(3500m, updated.BuyRate);
        Assert.AreEqual(HttpMethod.Put, handler.LastRequest!.Method);
        Assert.AreEqual("http://localhost/api/exchange-rates/USD", handler.LastRequest.RequestUri!.ToString());
    }

    [TestMethod]
    public async Task UpdateRateAsync_ServerError_ThrowsWithServerMessage()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.BadRequest, "Buy rate cannot exceed sell rate.");
        var client = CreateClient(handler);

        var ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => client.UpdateRateAsync("USD", buyRate: 3600m, sellRate: 3500m));
        Assert.AreEqual("Buy rate cannot exceed sell rate.", ex.Message);
    }

    private static ApiClient CreateClient(FakeHttpMessageHandler handler)
        => new(new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") });

    // Minimal fake handler: always returns the configured status/body, and
    // records the last request so tests can assert on method/URL.
    private sealed class FakeHttpMessageHandler(HttpStatusCode statusCode, string responseBody) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            LastRequest = request;
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseBody)
            };
            return Task.FromResult(response);
        }
    }
}