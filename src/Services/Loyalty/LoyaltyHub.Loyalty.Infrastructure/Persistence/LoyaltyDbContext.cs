using LoyaltyHub.Loyalty.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyHub.Loyalty.Infrastructure.Persistence;

public sealed class LoyaltyDbContext : DbContext
{
    public LoyaltyDbContext(DbContextOptions<LoyaltyDbContext> options)
        : base(options)
    {
    }

    public DbSet<ScoreCalculation> ScoreCalculations => Set<ScoreCalculation>();

    public DbSet<RecentPurchase> RecentPurchases => Set<RecentPurchase>();

    public DbSet<DbSeedVersion> DbSeedVersions => Set<DbSeedVersion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LoyaltyDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
