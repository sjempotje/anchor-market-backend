using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Markets.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Markets.Queries;

/// <summary>
/// Query to retrieve a market's most recent trades. Each bet placement records an
/// <see cref="AnchorMarket.Domain.Entities.OutcomePricePoint"/> for every outcome in the market, so
/// this reuses that table rather than a separate trade log.
/// </summary>
/// <param name="MarketId">The market ID.</param>
/// <param name="Limit">Maximum number of most-recent trades to return.</param>
/// <param name="CallerId">The authenticated caller, if any. Required to view group-scoped markets.</param>
public record GetMarketTradesQuery(Guid MarketId, int Limit = 50, Guid? CallerId = null)
    : IRequest<List<TradeDto>>;

/// <summary>Handles retrieving a market's trades, enforcing group membership for group-scoped markets.</summary>
public class GetMarketTradesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetMarketTradesQuery, List<TradeDto>>
{
    public async Task<List<TradeDto>> Handle(GetMarketTradesQuery request, CancellationToken cancellationToken)
    {
        var market = await context.Markets
            .Where(m => m.Id == request.MarketId)
            .Select(m => new { m.GroupId })
            .FirstOrDefaultAsync(cancellationToken);

        if (market is null)
            throw new NotFoundException("Market not found.");

        if (market.GroupId.HasValue)
        {
            var isMember = request.CallerId is { } callerId &&
                await context.GroupMemberships.AnyAsync(
                    m => m.GroupId == market.GroupId && m.UserId == callerId, cancellationToken);

            if (!isMember)
                throw new ForbiddenException("You are not a member of the group this market belongs to.");
        }

        return await context.OutcomePricePoints
            .Where(p => p.Outcome.MarketId == request.MarketId && p.IsTrade)
            .OrderByDescending(p => p.CreatedAt)
            .Take(request.Limit)
            .Select(p => new TradeDto(p.Id, p.OutcomeId, p.Price, p.Volume, p.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
