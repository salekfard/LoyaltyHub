using FluentValidation;
using LoyaltyHub.Loyalty.Application.DTOs;
using LoyaltyHub.Loyalty.Application.Interfaces;
using LoyaltyHub.Loyalty.Domain.Entities;
using MediatR;

namespace LoyaltyHub.Loyalty.Application.Features.Scoring.Queries;

public sealed record GetScoreCalculationByIdQuery(
    Guid Id,
    bool IncludeRecentPurchases = true) : IRequest<ScoreCalculationDto?>;

public sealed class GetScoreCalculationByIdQueryHandler
    : IRequestHandler<GetScoreCalculationByIdQuery, ScoreCalculationDto?>
{
    private readonly IScoreCalculationRepository _repository;

    public GetScoreCalculationByIdQueryHandler(IScoreCalculationRepository repository)
    {
        _repository = repository;
    }

    public async Task<ScoreCalculationDto?> Handle(
        GetScoreCalculationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(
            request.Id,
            request.IncludeRecentPurchases,
            cancellationToken);

        return entity is null
            ? null
            : ScoreCalculationMapper.ToDto(entity, request.IncludeRecentPurchases);
    }
}

public sealed class GetScoreCalculationByIdQueryValidator : AbstractValidator<GetScoreCalculationByIdQuery>
{
    public GetScoreCalculationByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id must not be empty.");
    }
}

internal static class ScoreCalculationMapper
{
    public static ScoreCalculationDto ToDto(ScoreCalculation entity, bool includeRecentPurchases)
    {
        IReadOnlyList<decimal>? recentPurchases = null;
        if (includeRecentPurchases)
        {
            recentPurchases = entity.RecentPurchases
                .OrderBy(x => x.Sequence)
                .Select(x => x.Amount)
                .ToList();
        }

        return new ScoreCalculationDto(
            entity.Id,
            entity.PurchaseAmount,
            entity.CustomerType,
            entity.PurchaseCount,
            entity.BaseScore,
            entity.FrequencyBonus,
            entity.HighPurchaseBonus,
            entity.TotalBonus,
            entity.FinalScore,
            entity.CreatedAtUtc,
            recentPurchases);
    }
}
