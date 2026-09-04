using FluentValidation;
using LoyaltyHub.Loyalty.Application.DTOs;
using LoyaltyHub.Loyalty.Application.Interfaces;
using MediatR;

namespace LoyaltyHub.Loyalty.Application.Features.Scoring.Queries;

public sealed record GetScoreCalculationsPagedQuery(
    int PageNumber = 1,
    int PageSize = 25,
    bool IncludeRecentPurchases = false) : IRequest<ScoreCalculationPagedResponseDto>;

public sealed class GetScoreCalculationsPagedQueryHandler
    : IRequestHandler<GetScoreCalculationsPagedQuery, ScoreCalculationPagedResponseDto>
{
    private readonly IScoreCalculationRepository _repository;

    public GetScoreCalculationsPagedQueryHandler(IScoreCalculationRepository repository)
    {
        _repository = repository;
    }

    public async Task<ScoreCalculationPagedResponseDto> Handle(
        GetScoreCalculationsPagedQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.IncludeRecentPurchases,
            cancellationToken);

        var dtos = items
            .Select(entity => ScoreCalculationMapper.ToDto(entity, request.IncludeRecentPurchases))
            .ToList();

        return new ScoreCalculationPagedResponseDto(
            dtos,
            totalCount,
            request.PageNumber,
            request.PageSize);
    }
}

public sealed class GetScoreCalculationsPagedQueryValidator : AbstractValidator<GetScoreCalculationsPagedQuery>
{
    public GetScoreCalculationsPagedQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("PageNumber must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");
    }
}
