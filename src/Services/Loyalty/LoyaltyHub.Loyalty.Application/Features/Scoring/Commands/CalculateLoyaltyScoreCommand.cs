using FluentValidation;
using LoyaltyHub.Loyalty.Application.DTOs;
using LoyaltyHub.Loyalty.Application.Interfaces;
using LoyaltyHub.Loyalty.Domain.Entities;
using LoyaltyHub.Loyalty.Domain.Enums;
using LoyaltyHub.Loyalty.Domain.Services;
using MediatR;

namespace LoyaltyHub.Loyalty.Application.Features.Scoring.Commands;

public sealed record CalculateLoyaltyScoreCommand(
    decimal PurchaseAmount,
    CustomerType CustomerType,
    IReadOnlyList<decimal> RecentPurchases) : IRequest<CalculateLoyaltyScoreResponseDto>;

public sealed class CalculateLoyaltyScoreCommandHandler
    : IRequestHandler<CalculateLoyaltyScoreCommand, CalculateLoyaltyScoreResponseDto>
{
    private readonly ILoyaltyScoringService _scoringService;
    private readonly IScoreCalculationRepository _repository;

    public CalculateLoyaltyScoreCommandHandler(
        ILoyaltyScoringService scoringService,
        IScoreCalculationRepository repository)
    {
        _scoringService = scoringService;
        _repository = repository;
    }

    public async Task<CalculateLoyaltyScoreResponseDto> Handle(
        CalculateLoyaltyScoreCommand request,
        CancellationToken cancellationToken)
    {
        var score = _scoringService.Calculate(
            request.PurchaseAmount,
            request.CustomerType,
            request.RecentPurchases);

        var createdAtUtc = DateTime.UtcNow;
        var entity = ScoreCalculation.Create(
            score.PurchaseAmount,
            score.CustomerType,
            request.RecentPurchases,
            score.BaseScore,
            score.FrequencyBonus,
            score.HighPurchaseBonus,
            score.TotalBonus,
            score.FinalScore,
            createdAtUtc);

        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return new CalculateLoyaltyScoreResponseDto(
            entity.Id,
            entity.PurchaseAmount,
            entity.CustomerType,
            entity.PurchaseCount,
            entity.BaseScore,
            entity.FrequencyBonus,
            entity.HighPurchaseBonus,
            entity.TotalBonus,
            entity.FinalScore,
            entity.CreatedAtUtc);
    }
}

public sealed class CalculateLoyaltyScoreCommandValidator : AbstractValidator<CalculateLoyaltyScoreCommand>
{
    public CalculateLoyaltyScoreCommandValidator()
    {
        RuleFor(x => x.PurchaseAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CustomerType)
            .Must(type => type is CustomerType.Bronze or CustomerType.Silver or CustomerType.Gold)
            .WithMessage("CustomerType must be Bronze, Silver, or Gold.");

        RuleFor(x => x.RecentPurchases)
            .NotNull()
            .Must(list => list is { Count: > 0 })
            .WithMessage("RecentPurchases must contain at least one amount.");

        RuleForEach(x => x.RecentPurchases)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x)
            .Must(x => x.RecentPurchases is not null && x.RecentPurchases.Sum() == x.PurchaseAmount)
            .WithMessage("PurchaseAmount must equal the sum of RecentPurchases.");
    }
}
