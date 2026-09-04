using LoyaltyHub.Loyalty.Domain.Enums;

namespace LoyaltyHub.Loyalty.Domain.Services;

public sealed class LoyaltyScoreResult
{
    public required decimal BaseScore { get; init; }

    public required decimal FrequencyBonus { get; init; }

    public required decimal HighPurchaseBonus { get; init; }

    public required decimal TotalBonus { get; init; }

    public required decimal FinalScore { get; init; }

    public required CustomerType CustomerType { get; init; }

    public required decimal PurchaseAmount { get; init; }

    public required int PurchaseCount { get; init; }
}
