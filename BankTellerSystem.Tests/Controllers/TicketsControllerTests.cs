using System.Net;
using System.Net.Http.Json;
using BankTellerSystem.Server.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BankTellerSystem.Tests.Controllers;

[TestClass]
public class TicketsControllerTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private string _dbPath = null!;

    [TestInitialize]
    public void Setup()
    {
        // Point each test run at its own throwaway SQLite file so tests don't
        // collide with each other or with a real dev database.
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
    public async Task IssueTicket_ReturnsNewTicket_StartingAtA001()
    {
        var response = await _client.PostAsync("/api/tickets", null);
        response.EnsureSuccessStatusCode();

        var ticket = await response.Content.ReadFromJsonAsync<TicketDto>();

        Assert.IsNotNull(ticket);
        Assert.AreEqual("A001", ticket.Number);
    }

    [TestMethod]
    public async Task CallNext_WithWaitingTicket_ReturnsIt()
    {
        await _client.PostAsync("/api/tickets", null); // issue A001

        var response = await _client.PostAsync("/api/tickets/call-next?counterId=1", null);
        response.EnsureSuccessStatusCode();

        var ticket = await response.Content.ReadFromJsonAsync<TicketDto>();

        Assert.IsNotNull(ticket);
        Assert.AreEqual("A001", ticket.Number);
        Assert.AreEqual(1, ticket.CalledByCounterId);
    }

    [TestMethod]
    public async Task CallNext_WithNoWaitingTickets_ReturnsNoContent()
    {
        var response = await _client.PostAsync("/api/tickets/call-next?counterId=1", null);

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }

    [TestMethod]
    public async Task CallNext_UnknownCounter_ReturnsBadRequest()
    {
        var response = await _client.PostAsync("/api/tickets/call-next?counterId=999", null);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task CompleteCurrent_AfterCallNext_MarksTicketServed()
    {
        await _client.PostAsync("/api/tickets", null);
        await _client.PostAsync("/api/tickets/call-next?counterId=1", null);

        var response = await _client.PostAsync("/api/tickets/complete-current?counterId=1", null);
        response.EnsureSuccessStatusCode();

        var ticket = await response.Content.ReadFromJsonAsync<TicketDto>();

        Assert.IsNotNull(ticket);
        Assert.AreEqual(Domain.TicketStatus.Served, ticket.Status);
    }
}