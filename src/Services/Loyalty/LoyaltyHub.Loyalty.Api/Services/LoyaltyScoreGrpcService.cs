using Grpc.Core;
using LoyaltyHub.Loyalty.Api.Grpc;
using LoyaltyHub.Loyalty.Application.DTOs;
using LoyaltyHub.Loyalty.Application.Features.Scoring.Commands;
using LoyaltyHub.Loyalty.Application.Features.Scoring.Queries;
using MediatR;
using DomainCustomerType = LoyaltyHub.Loyalty.Domain.Enums.CustomerType;
using ProtoCustomerType = LoyaltyHub.Loyalty.Api.Grpc.CustomerType;

namespace LoyaltyHub.Loyalty.Api.Services;

public sealed class LoyaltyScoreGrpcService : LoyaltyScore.LoyaltyScoreBase
{
    private readonly ISender _sender;

    public LoyaltyScoreGrpcService(ISender sender)
    {
        _sender = sender;
    }

    public override async Task<CalculateScoreReply> CalculateScore(
        CalculateScoreRequest request,
        ServerCallContext context)
    {
        var customerType = MapCustomerType(request.CustomerType);
        var recentPurchases = request.RecentPurchases
            .Select(amount => Convert.ToDecimal(amount))
            .ToList();

        var result = await _sender.Send(
            new CalculateLoyaltyScoreCommand(
                Convert.ToDecimal(request.PurchaseAmount),
                customerType,
                recentPurchases),
            context.CancellationToken);

        return new CalculateScoreReply
        {
            CalculationId = result.CalculationId.ToString(),
            PurchaseAmount = Convert.ToDouble(result.PurchaseAmount),
            CustomerType = MapToProto(result.CustomerType),
            PurchaseCount = result.PurchaseCount,
            BaseScore = Convert.ToDouble(result.BaseScore),
            FrequencyBonus = Convert.ToDouble(result.FrequencyBonus),
            HighPurchaseBonus = Convert.ToDouble(result.HighPurchaseBonus),
            TotalBonus = Convert.ToDouble(result.TotalBonus),
            FinalScore = Convert.ToDouble(result.FinalScore),
            CreatedAtUtc = result.CreatedAtUtc.ToString("O")
        };
    }

    public override async Task<ScoreCalculationReply> GetScoreCalculationById(
        GetScoreCalculationByIdRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Id must be a valid GUID."));
        }

        var includeRecentPurchases = request.HasIncludeRecentPurchases
            ? request.IncludeRecentPurchases
            : true;

        var result = await _sender.Send(
            new GetScoreCalculationByIdQuery(id, includeRecentPurchases),
            context.CancellationToken);

        if (result is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Score calculation '{id}' was not found."));
        }

        return MapToScoreCalculationReply(result);
    }

    public override async Task<GetScoreCalculationsPagedReply> GetScoreCalculationsPaged(
        GetScoreCalculationsPagedRequest request,
        ServerCallContext context)
    {
        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 25 : request.PageSize;
        var includeRecentPurchases = request.HasIncludeRecentPurchases
            && request.IncludeRecentPurchases;

        var result = await _sender.Send(
            new GetScoreCalculationsPagedQuery(pageNumber, pageSize, includeRecentPurchases),
            context.CancellationToken);

        var reply = new GetScoreCalculationsPagedReply
        {
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };

        reply.Items.AddRange(result.Items.Select(MapToScoreCalculationReply));
        return reply;
    }

    private static ScoreCalculationReply MapToScoreCalculationReply(ScoreCalculationDto dto)
    {
        var reply = new ScoreCalculationReply
        {
            CalculationId = dto.CalculationId.ToString(),
            PurchaseAmount = Convert.ToDouble(dto.PurchaseAmount),
            CustomerType = MapToProto(dto.CustomerType),
            PurchaseCount = dto.PurchaseCount,
            BaseScore = Convert.ToDouble(dto.BaseScore),
            FrequencyBonus = Convert.ToDouble(dto.FrequencyBonus),
            HighPurchaseBonus = Convert.ToDouble(dto.HighPurchaseBonus),
            TotalBonus = Convert.ToDouble(dto.TotalBonus),
            FinalScore = Convert.ToDouble(dto.FinalScore),
            CreatedAtUtc = dto.CreatedAtUtc.ToString("O")
        };

        if (dto.RecentPurchases is not null)
        {
            reply.RecentPurchases.AddRange(dto.RecentPurchases.Select(Convert.ToDouble));
        }

        return reply;
    }

    private static DomainCustomerType MapCustomerType(ProtoCustomerType customerType) =>
        customerType switch
        {
            ProtoCustomerType.Bronze => DomainCustomerType.Bronze,
            ProtoCustomerType.Silver => DomainCustomerType.Silver,
            ProtoCustomerType.Gold => DomainCustomerType.Gold,
            _ => DomainCustomerType.Unknown
        };

    private static ProtoCustomerType MapToProto(DomainCustomerType customerType) =>
        customerType switch
        {
            DomainCustomerType.Bronze => ProtoCustomerType.Bronze,
            DomainCustomerType.Silver => ProtoCustomerType.Silver,
            DomainCustomerType.Gold => ProtoCustomerType.Gold,
            _ => ProtoCustomerType.Unspecified
        };
}
