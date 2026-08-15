using BankTellerSystem.Server.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF Core + SQLite. We use IDbContextFactory (not AddDbContext) because our
// services will be registered as singletons and create a short-lived
// DbContext per operation - the recommended pattern outside per-request scope.
var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=bankteller.db";
builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(connectionString));

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

app.Run();

// Exposed for WebApplicationFactory in integration tests, if we need it later.
public partial class Program { }