using LoyaltyHub.Loyalty.Domain.Entities;

namespace LoyaltyHub.Loyalty.Application.Interfaces;

// Repository pattern: Application owns this persistence contract; Infrastructure implements it with EF Core.
public interface IScoreCalculationRepository
{
    Task AddAsync(ScoreCalculation calculation, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<ScoreCalculation?> GetByIdAsync(
        Guid id,
        bool includeRecentPurchases,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<ScoreCalculation> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        bool includeRecentPurchases,
        CancellationToken cancellationToken = default);
}
