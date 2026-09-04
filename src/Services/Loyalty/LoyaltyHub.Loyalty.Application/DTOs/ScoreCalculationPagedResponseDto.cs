namespace LoyaltyHub.Loyalty.Application.DTOs;

public sealed record ScoreCalculationPagedResponseDto(
    IReadOnlyList<ScoreCalculationDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);
