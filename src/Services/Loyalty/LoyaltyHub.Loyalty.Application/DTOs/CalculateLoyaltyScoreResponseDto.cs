using LoyaltyHub.Loyalty.Domain.Enums;

namespace LoyaltyHub.Loyalty.Application.DTOs;

public sealed record CalculateLoyaltyScoreResponseDto(
    Guid CalculationId,
    decimal PurchaseAmount,
    CustomerType CustomerType,
    int PurchaseCount,
    decimal BaseScore,
    decimal FrequencyBonus,
    decimal HighPurchaseBonus,
    decimal TotalBonus,
    decimal FinalScore,
    DateTime CreatedAtUtc);
