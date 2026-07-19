using EventGateway.Models;
using Microsoft.EntityFrameworkCore;

namespace EventGateway.Data;

public sealed class EventLedgerDbContext : DbContext
{
    public EventLedgerDbContext(DbContextOptions<EventLedgerDbContext> options)
        : base(options)
    {
    }

    public DbSet<EventEntity> Events => Set<EventEntity>();
    public DbSet<AccountBalanceEntity> AccountBalances => Set<AccountBalanceEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventEntity>()
            .HasIndex(e => e.EventId)
            .IsUnique();

        modelBuilder.Entity<AccountBalanceEntity>()
            .HasIndex(a => a.AccountId)
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}
