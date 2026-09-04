using LoyaltyHub.Loyalty.Domain.Enums;

namespace LoyaltyHub.Loyalty.Domain.Entities;

// DDD entity: private setters and a factory keep creation and state changes inside the domain.
public class ScoreCalculation : EntityBase
{
    private readonly List<RecentPurchase> _recentPurchases = [];

    public decimal PurchaseAmount { get; private set; }

    public CustomerType CustomerType { get; private set; }

    public IReadOnlyCollection<RecentPurchase> RecentPurchases => _recentPurchases.AsReadOnly();

    public int PurchaseCount { get; private set; }

    public decimal BaseScore { get; private set; }

    public decimal FrequencyBonus { get; private set; }

    public decimal HighPurchaseBonus { get; private set; }

    public decimal TotalBonus { get; private set; }

    public decimal FinalScore { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private ScoreCalculation()
    {
    }

    public static ScoreCalculation Create(
        decimal purchaseAmount,
        CustomerType customerType,
        IReadOnlyList<decimal> recentPurchases,
        decimal baseScore,
        decimal frequencyBonus,
        decimal highPurchaseBonus,
        decimal totalBonus,
        decimal finalScore,
        DateTime createdAtUtc)
    {
        var entity = new ScoreCalculation
        {
            PurchaseAmount = purchaseAmount,
            CustomerType = customerType,
            PurchaseCount = recentPurchases.Count,
            BaseScore = baseScore,
            FrequencyBonus = frequencyBonus,
            HighPurchaseBonus = highPurchaseBonus,
            TotalBonus = totalBonus,
            FinalScore = finalScore,
            CreatedAtUtc = DateTime.SpecifyKind(createdAtUtc, DateTimeKind.Utc)
        };

        for (var i = 0; i < recentPurchases.Count; i++)
        {
            entity._recentPurchases.Add(RecentPurchase.Create(recentPurchases[i], i));
        }

        return entity;
    }
}
