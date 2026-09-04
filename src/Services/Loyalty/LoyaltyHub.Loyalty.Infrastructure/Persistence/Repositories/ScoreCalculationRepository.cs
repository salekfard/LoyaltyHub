using LoyaltyHub.Loyalty.Application.Interfaces;
using LoyaltyHub.Loyalty.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyHub.Loyalty.Infrastructure.Persistence.Repositories;

public sealed class ScoreCalculationRepository : IScoreCalculationRepository
{
    private readonly LoyaltyDbContext _dbContext;

    public ScoreCalculationRepository(LoyaltyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ScoreCalculation calculation, CancellationToken cancellationToken = default)
    {
        await _dbContext.ScoreCalculations.AddAsync(calculation, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ScoreCalculation?> GetByIdAsync(
        Guid id,
        bool includeRecentPurchases,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ScoreCalculations.AsQueryable();

        if (includeRecentPurchases)
        {
            query = query.Include(x => x.RecentPurchases);
        }

        return await query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<ScoreCalculation> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        bool includeRecentPurchases,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await _dbContext.ScoreCalculations.CountAsync(cancellationToken);

        var query = _dbContext.ScoreCalculations.AsQueryable();

        if (includeRecentPurchases)
        {
            query = query.Include(x => x.RecentPurchases);
        }

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
