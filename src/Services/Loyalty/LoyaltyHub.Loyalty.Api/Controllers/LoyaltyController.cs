using LoyaltyHub.Loyalty.Application.DTOs;
using LoyaltyHub.Loyalty.Application.Features.Scoring.Commands;
using LoyaltyHub.Loyalty.Application.Features.Scoring.Queries;
using LoyaltyHub.Loyalty.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyHub.Loyalty.Api.Controllers;

[ApiController]
[Route("api/loyalty")]
public sealed class LoyaltyController : ControllerBase
{
    private readonly ISender _sender;

    public LoyaltyController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Calculates the loyalty final score and stores the calculation history.
    /// </summary>
    [HttpPost("score")]
    [ProducesResponseType(typeof(CalculateLoyaltyScoreHttpResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CalculateLoyaltyScoreHttpResponse>> CalculateScore(
        [FromBody] CalculateLoyaltyScoreHttpRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CalculateLoyaltyScoreCommand(
                request.PurchaseAmount,
                request.CustomerType,
                request.RecentPurchases),
            cancellationToken);

        return Ok(new CalculateLoyaltyScoreHttpResponse(
            result.CalculationId,
            result.PurchaseAmount,
            result.CustomerType,
            result.PurchaseCount,
            result.BaseScore,
            result.FrequencyBonus,
            result.HighPurchaseBonus,
            result.TotalBonus,
            result.FinalScore,
            result.CreatedAtUtc));
    }

    /// <summary>
    /// Gets a score calculation by id.
    /// </summary>
    [HttpGet("score-calculations/{id:guid}")]
    [ProducesResponseType(typeof(ScoreCalculationHttpResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ScoreCalculationHttpResponse>> GetScoreCalculationById(
        Guid id,
        [FromQuery] bool includeRecentPurchases = true,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetScoreCalculationByIdQuery(id, includeRecentPurchases),
            cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(MapToHttpResponse(result));
    }

    /// <summary>
    /// Gets a paged list of score calculations.
    /// </summary>
    [HttpGet("score-calculations")]
    [ProducesResponseType(typeof(ScoreCalculationPagedHttpResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ScoreCalculationPagedHttpResponse>> GetScoreCalculationsPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] bool includeRecentPurchases = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetScoreCalculationsPagedQuery(pageNumber, pageSize, includeRecentPurchases),
            cancellationToken);

        return Ok(new ScoreCalculationPagedHttpResponse(
            result.Items.Select(MapToHttpResponse).ToList(),
            result.TotalCount,
            result.PageNumber,
            result.PageSize));
    }

    private static ScoreCalculationHttpResponse MapToHttpResponse(ScoreCalculationDto dto) =>
        new(
            dto.CalculationId,
            dto.PurchaseAmount,
            dto.CustomerType,
            dto.PurchaseCount,
            dto.BaseScore,
            dto.FrequencyBonus,
            dto.HighPurchaseBonus,
            dto.TotalBonus,
            dto.FinalScore,
            dto.CreatedAtUtc,
            dto.RecentPurchases);
}

public sealed record CalculateLoyaltyScoreHttpRequest(
    decimal PurchaseAmount,
    CustomerType CustomerType,
    IReadOnlyList<decimal> RecentPurchases);

public sealed record CalculateLoyaltyScoreHttpResponse(
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

public sealed record ScoreCalculationHttpResponse(
    Guid CalculationId,
    decimal PurchaseAmount,
    CustomerType CustomerType,
    int PurchaseCount,
    decimal BaseScore,
    decimal FrequencyBonus,
    decimal HighPurchaseBonus,
    decimal TotalBonus,
    decimal FinalScore,
    DateTime CreatedAtUtc,
    IReadOnlyList<decimal>? RecentPurchases);

public sealed record ScoreCalculationPagedHttpResponse(
    IReadOnlyList<ScoreCalculationHttpResponse> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);
