using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.MarketResolutions.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.MarketResolutions.Queries;

/// <summary>Query to retrieve a market's resolution (winner), if it has been resolved.</summary>
/// <param name="MarketId">The market ID.</param>
/// <param name="CallerId">The authenticated caller, if any. Required to view group-scoped markets.</param>
public record GetMarketResolutionQuery(Guid MarketId, Guid? CallerId = null) : IRequest<MarketResolutionDto?>;

/// <summary>Handles retrieving a market's resolution, enforcing group membership for group-scoped markets.</summary>
public class GetMarketResolutionQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetMarketResolutionQuery, MarketResolutionDto?>
{
    /// <summary>Returns the market's resolution, or null if it is not yet resolved.</summary>
    public async Task<MarketResolutionDto?> Handle(GetMarketResolutionQuery request, CancellationToken cancellationToken)
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

        return await context.MarketResolutions
            .Where(r => r.MarketId == request.MarketId)
            .ProjectTo<MarketResolutionDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
