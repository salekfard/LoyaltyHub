using LoyaltyHub.Loyalty.Domain.Enums;

namespace LoyaltyHub.Loyalty.Domain.Services;

public interface ILoyaltyScoringService
{
    LoyaltyScoreResult Calculate(
        decimal purchaseAmount,
        CustomerType customerType,
        IReadOnlyList<decimal> recentPurchases);
}
