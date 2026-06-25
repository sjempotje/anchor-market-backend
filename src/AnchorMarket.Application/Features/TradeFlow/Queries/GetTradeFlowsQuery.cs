using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.TradeFlow.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.TradeFlow.Queries;

/// <summary>Query to retrieve recent trade flow snapshots for a market, oldest first.</summary>
public record GetTradeFlowsQuery(Guid MarketId, int Limit = 200) : IRequest<List<TradeFlowDto>>;

/// <summary>Handles retrieving trade flow snapshots for a market.</summary>
public class GetTradeFlowsQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetTradeFlowsQuery, List<TradeFlowDto>>
{
    /// <summary>Returns the most recent trade flows for the market, ordered chronologically.</summary>
    public async Task<List<TradeFlowDto>> Handle(GetTradeFlowsQuery request, CancellationToken cancellationToken)
    {
        var flows = await context.TradeFlowSnapshots
            .Where(s => s.MarketId == request.MarketId)
            .OrderByDescending(s => s.Timestamp)
            .Take(Math.Clamp(request.Limit, 1, 2000))
            .ProjectTo<TradeFlowDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        return [.. flows.OrderBy(s => s.Timestamp)];
    }
}
