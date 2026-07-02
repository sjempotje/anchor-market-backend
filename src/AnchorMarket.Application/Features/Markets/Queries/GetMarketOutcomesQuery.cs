using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Markets.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Markets.Queries;

/// <summary>Query to retrieve the outcomes of a market, ordered for display.</summary>
/// <param name="MarketId">The market ID.</param>
/// <param name="CallerId">The authenticated caller, if any. Required to view group-scoped markets.</param>
public record GetMarketOutcomesQuery(Guid MarketId, Guid? CallerId = null) : IRequest<List<OutcomeDto>>;

/// <summary>Handles retrieving a market's outcomes, enforcing group membership for group-scoped markets.</summary>
public class GetMarketOutcomesQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetMarketOutcomesQuery, List<OutcomeDto>>
{
    /// <summary>Returns the market's outcomes ordered by sort order.</summary>
    public async Task<List<OutcomeDto>> Handle(GetMarketOutcomesQuery request, CancellationToken cancellationToken)
    {
        var market = await context.Markets
            .Where(m => m.Id == request.MarketId)
            .Select(m => new { m.GroupId })
            .FirstOrDefaultAsync(cancellationToken);

        if (market is not null && market.GroupId.HasValue)
        {
            var isMember = request.CallerId is { } callerId &&
                await context.GroupMemberships.AnyAsync(
                    m => m.GroupId == market.GroupId && m.UserId == callerId, cancellationToken);

            if (!isMember)
                throw new ForbiddenException("You are not a member of the group this market belongs to.");
        }

        return await context.Outcomes
            .Where(o => o.MarketId == request.MarketId)
            .OrderBy(o => o.SortOrder)
            .ProjectTo<OutcomeDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
