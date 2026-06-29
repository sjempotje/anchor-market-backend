using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.OrderBookHistory.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.OrderBookHistory.Queries;

/// <summary>Query to retrieve recent order book snapshots for an outcome, oldest first.</summary>
public record GetOrderBookSnapshotsQuery(Guid OutcomeId, int Limit = 200) : IRequest<List<OrderBookSnapshotDto>>;

/// <summary>Handles retrieving order book snapshots for an outcome.</summary>
public class GetOrderBookSnapshotsQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetOrderBookSnapshotsQuery, List<OrderBookSnapshotDto>>
{
    /// <summary>Returns the most recent snapshots for the outcome, ordered chronologically.</summary>
    public async Task<List<OrderBookSnapshotDto>> Handle(GetOrderBookSnapshotsQuery request, CancellationToken cancellationToken)
    {
        var snapshots = await context.OrderBookSnapshots
            .Where(s => s.OutcomeId == request.OutcomeId)
            .ProjectTo<OrderBookSnapshotDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        return [.. snapshots
            .OrderByDescending(s => s.Timestamp)
            .Take(Math.Clamp(request.Limit, 1, 2000))
            .OrderBy(s => s.Timestamp)];
    }
}
