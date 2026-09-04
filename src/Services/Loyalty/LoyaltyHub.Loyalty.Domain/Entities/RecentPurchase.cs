namespace LoyaltyHub.Loyalty.Domain.Entities;

public class RecentPurchase : EntityBase
{
    public Guid ScoreCalculationId { get; private set; }

    public decimal Amount { get; private set; }

    public int Sequence { get; private set; }

    public ScoreCalculation ScoreCalculation { get; private set; } = null!;

    private RecentPurchase()
    {
    }

    internal static RecentPurchase Create(decimal amount, int sequence)
    {
        return new RecentPurchase
        {
            Amount = amount,
            Sequence = sequence
        };
    }
}
