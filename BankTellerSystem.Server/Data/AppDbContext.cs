using BankTellerSystem.Domain;
using Microsoft.EntityFrameworkCore;

namespace BankTellerSystem.Server.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Counter> Counters => Set<Counter>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // SQLite has no native decimal type - be explicit about precision/scale.
        modelBuilder.Entity<Account>().Property(a => a.Balance).HasPrecision(18, 2);
        modelBuilder.Entity<Transaction>().Property(t => t.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<ExchangeRate>().Property(r => r.BuyRate).HasPrecision(18, 2);
        modelBuilder.Entity<ExchangeRate>().Property(r => r.SellRate).HasPrecision(18, 2);

        // Seed starter data so the API is usable immediately after first run.
        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Counter>().HasData(
            new Counter { Id = 1, Name = "Counter 1" },
            new Counter { Id = 2, Name = "Counter 2" },
            new Counter { Id = 3, Name = "Counter 3" });

        modelBuilder.Entity<Account>().HasData(
            new Account { Id = 1, AccountNumber = "1000000001", OwnerName = "Bat", Balance = 1_000_000m },
            new Account { Id = 2, AccountNumber = "1000000002", OwnerName = "Bold", Balance = 500_000m });

        modelBuilder.Entity<ExchangeRate>().HasData(
            new ExchangeRate { Id = 1, CurrencyCode = "USD", BuyRate = 3450, SellRate = 3470, UpdatedAtUtc = seedDate },
            new ExchangeRate { Id = 2, CurrencyCode = "EUR", BuyRate = 3700, SellRate = 3730, UpdatedAtUtc = seedDate },
            new ExchangeRate { Id = 3, CurrencyCode = "CNY", BuyRate = 480, SellRate = 490, UpdatedAtUtc = seedDate });
    }
}