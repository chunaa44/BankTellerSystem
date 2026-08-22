using BankTellerSystem.Server.Data;
using BankTellerSystem.Server.Queueing;
using BankTellerSystem.Server.Realtime;
using BankTellerSystem.Server.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// EF Core + SQLite. We use IDbContextFactory (not AddDbContext) because our
// services will be registered as singletons and create a short-lived
// DbContext per operation
var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=bankteller.db";
builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(connectionString));

// Single shared queue so ticket, transfer, and rate operations are all
// serialized against each other, not just within their own service.
builder.Services.AddSingleton<SerialOperationQueue>();
builder.Services.AddSingleton<TicketQueueService>();
builder.Services.AddSingleton<AccountTransferService>();
builder.Services.AddSingleton<ExchangeRateService>();

// Tracks connected ticket-display screens and lets us broadcast to them.
builder.Services.AddSingleton<TicketDisplayTcpConnectionManager>();

// Starts the TCP listener as soon as the app starts, stops it on shutdown.
builder.Services.AddHostedService<TicketDisplayTcpServer>();

var app = builder.Build();

// Create the SQLite database (and apply seed data) on first run.
using (var scope = app.Services.CreateScope())
{
    var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using var db = await dbFactory.CreateDbContextAsync();
    await db.Database.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHub<ExchangeRateHub>("/hubs/exchange-rates");

app.Run();


// Exposed for WebApplicationFactory in integration tests
public partial class Program { }

