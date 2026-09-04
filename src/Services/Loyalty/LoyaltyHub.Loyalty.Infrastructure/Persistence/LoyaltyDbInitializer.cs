using LoyaltyHub.Loyalty.Domain.Entities;
using LoyaltyHub.Loyalty.Domain.Enums;
using LoyaltyHub.Loyalty.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LoyaltyHub.Loyalty.Infrastructure.Persistence;

public sealed class LoyaltyDbInitializer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LoyaltyDbInitializer> _logger;

    public LoyaltyDbInitializer(IServiceProvider serviceProvider, ILogger<LoyaltyDbInitializer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task InitialiseAsync(
        bool seedSampleData = false,
        CancellationToken cancellationToken = default)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LoyaltyDbContext>();
        var connectionString = dbContext.Database.GetConnectionString()
            ?? throw new InvalidOperationException("Loyalty database connection string is missing.");

        _logger.LogInformation("Ensuring SQL Server database exists.");
        await SqlServerDatabaseEnsurer.EnsureDatabaseExistsAsync(connectionString, cancellationToken);

        _logger.LogInformation("Applying EF Core migrations.");
        await dbContext.Database.MigrateAsync(cancellationToken);

        if (!seedSampleData)
        {
            return;
        }

        await SeedSampleDataAsync(scope.ServiceProvider, dbContext, cancellationToken);
    }

    private async Task SeedSampleDataAsync(
        IServiceProvider serviceProvider,
        LoyaltyDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (await GetMaxDataSeedVersionAsync(dbContext, cancellationToken) >= 1)
        {
            _logger.LogInformation("Sample seed skipped; DbSeedVersion V1 already applied.");
            return;
        }

        var scoringService = serviceProvider.GetRequiredService<ILoyaltyScoringService>();
        var createdAtUtc = DateTime.UtcNow;

        var samples = new (decimal PurchaseAmount, CustomerType CustomerType, IReadOnlyList<decimal> RecentPurchases)[]
        {
            (500_000m, CustomerType.Bronze, [500_000m]),
            (2_000_000m, CustomerType.Silver, [400_000m, 400_000m, 400_000m, 400_000m, 400_000m]),
            (20_000_001m, CustomerType.Gold, [4_000_000m, 4_000_000m, 4_000_000m, 4_000_000m, 4_000_001m]),
            (10_000_001m, CustomerType.Bronze, [10_000_001m]),
            (1_500_000m, CustomerType.Gold, [500_000m, 500_000m, 500_000m])
        };

        var entities = new List<ScoreCalculation>(samples.Length);
        foreach (var sample in samples)
        {
            var score = scoringService.Calculate(
                sample.PurchaseAmount,
                sample.CustomerType,
                sample.RecentPurchases);

            entities.Add(ScoreCalculation.Create(
                score.PurchaseAmount,
                score.CustomerType,
                sample.RecentPurchases,
                score.BaseScore,
                score.FrequencyBonus,
                score.HighPurchaseBonus,
                score.TotalBonus,
                score.FinalScore,
                createdAtUtc));
        }

        // Don't call SaveChanges before AddDataSeedVersion — one unit of work for samples + version.
        await dbContext.ScoreCalculations.AddRangeAsync(entities, cancellationToken);
        await AddDataSeedVersionAsync(dbContext, 1, "Sample score calculations", cancellationToken);

        _logger.LogInformation("Seeded {Count} sample score calculations (DbSeedVersion V1).", entities.Count);
    }

    private static async Task<int> GetMaxDataSeedVersionAsync(
        LoyaltyDbContext dbContext,
        CancellationToken cancellationToken)
    {
        return await dbContext.DbSeedVersions
            .MaxAsync(v => (int?)v.Version, cancellationToken) ?? 0;
    }

    private static async Task AddDataSeedVersionAsync(
        LoyaltyDbContext dbContext,
        uint version,
        string description,
        CancellationToken cancellationToken)
    {
        dbContext.DbSeedVersions.Add(DbSeedVersion.Create(version, description, DateTime.UtcNow));
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
