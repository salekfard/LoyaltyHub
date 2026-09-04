using LoyaltyHub.Loyalty.Domain.Enums;

namespace LoyaltyHub.Loyalty.Domain.Services;

/// <summary>
/// DDD domain service: scoring formulas and bonus rules live here, not in Api or Application handlers.
/// </summary>
public sealed class LoyaltyScoringService : ILoyaltyScoringService
{
    public const decimal HighPurchaseThreshold = 10_000_000m;
    public const decimal FirstHighPurchaseBonus = 0.05m;
    public const decimal ExtraHighPurchaseBonusPerStep = 0.025m;
    public const decimal FrequencyBonusRate = 0.10m;
    public const int FrequencyBonusMinPurchases = 5;

    public LoyaltyScoreResult Calculate(
        decimal purchaseAmount,
        CustomerType customerType,
        IReadOnlyList<decimal> recentPurchases)
    {
        ArgumentNullException.ThrowIfNull(recentPurchases);

        if (purchaseAmount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(purchaseAmount), "Purchase amount cannot be negative.");
        }

        if (customerType is not (CustomerType.Bronze or CustomerType.Silver or CustomerType.Gold))
        {
            throw new ArgumentOutOfRangeException(nameof(customerType), "Customer type must be Bronze, Silver, or Gold.");
        }

        var baseScore = customerType switch
        {
            CustomerType.Bronze => purchaseAmount / 100m,
            CustomerType.Silver => purchaseAmount / 80m,
            CustomerType.Gold => purchaseAmount / 60m,
            _ => throw new ArgumentOutOfRangeException(nameof(customerType))
        };

        var frequencyBonus = recentPurchases.Count >= FrequencyBonusMinPurchases
            ? FrequencyBonusRate
            : 0m;

        var highPurchaseBonus = CalculateHighPurchaseBonus(purchaseAmount);
        var totalBonus = frequencyBonus + highPurchaseBonus;
        var finalScore = baseScore * (1m + totalBonus);

        return new LoyaltyScoreResult
        {
            PurchaseAmount = purchaseAmount,
            CustomerType = customerType,
            PurchaseCount = recentPurchases.Count,
            BaseScore = baseScore,
            FrequencyBonus = frequencyBonus,
            HighPurchaseBonus = highPurchaseBonus,
            TotalBonus = totalBonus,
            FinalScore = finalScore
        };
    }

    /// <summary>
    /// Applies the stepped high-purchase bonus on the total purchase amount.
    /// Amount must be strictly greater than 10,000,000.
    /// First step: 5%. Each additional full 10,000,000: +2.5%.
    /// </summary>
    public static decimal CalculateHighPurchaseBonus(decimal purchaseAmount)
    {
        if (purchaseAmount <= HighPurchaseThreshold)
        {
            return 0m;
        }

        var steps = (int)decimal.Floor(purchaseAmount / HighPurchaseThreshold);
        if (steps <= 0)
        {
            return 0m;
        }

        return FirstHighPurchaseBonus + ((steps - 1) * ExtraHighPurchaseBonusPerStep);
    }
}
