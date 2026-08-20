using System.Net;
using BankTellerSystem.DispenserApp;

namespace BankTellerSystem.Tests.DispenserApp;

[TestClass]
public class TicketApiClientTests
{
    [TestMethod]
    public async Task IssueTicketAsync_ReturnsTicketFromServerResponse()
    {
        var handler = new FakeHttpMessageHandler(
            HttpStatusCode.OK,
            """{"id":1,"number":"A001"}""");
        var client = new TicketApiClient(new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") });

        var ticket = await client.IssueTicketAsync();

        Assert.AreEqual(1, ticket.Id);
        Assert.AreEqual("A001", ticket.Number);
    }

    [TestMethod]
    public async Task IssueTicketAsync_PostsToTicketsEndpoint()
    {
        var handler = new FakeHttpMessageHandler(
            HttpStatusCode.OK,
            """{"id":1,"number":"A001"}""");
        var client = new TicketApiClient(new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") });

        await client.IssueTicketAsync();

        Assert.AreEqual(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.AreEqual("http://localhost/api/tickets", handler.LastRequest.RequestUri!.ToString());
    }

    [TestMethod]
    public async Task IssueTicketAsync_ServerError_Throws()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.InternalServerError, "");
        var client = new TicketApiClient(new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") });

        await Assert.ThrowsExactlyAsync<HttpRequestException>(() => client.IssueTicketAsync());
    }

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