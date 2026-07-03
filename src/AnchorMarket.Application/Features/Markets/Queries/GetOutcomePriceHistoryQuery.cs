using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Markets.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Markets.Queries;

/// <summary>Query to retrieve an outcome's historical implied-probability price series.</summary>
/// <param name="OutcomeId">The outcome ID.</param>
/// <param name="Limit">Maximum number of most-recent points to return.</param>
/// <param name="CallerId">The authenticated caller, if any. Required to view group-scoped markets.</param>
public record GetOutcomePriceHistoryQuery(Guid OutcomeId, int Limit = 500, Guid? CallerId = null)
    : IRequest<List<PricePointDto>>;

/// <summary>Handles retrieving an outcome's price history, enforcing group membership for group-scoped markets.</summary>
public class GetOutcomePriceHistoryQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetOutcomePriceHistoryQuery, List<PricePointDto>>
{
    public async Task<List<PricePointDto>> Handle(GetOutcomePriceHistoryQuery request, CancellationToken cancellationToken)
    {
        var outcome = await context.Outcomes
            .Where(o => o.Id == request.OutcomeId)
            .Select(o => new { o.MarketId, GroupId = o.Market.GroupId })
            .FirstOrDefaultAsync(cancellationToken);

        if (outcome is null)
            throw new NotFoundException("Outcome not found.");

        if (outcome.GroupId.HasValue)
        {
            var isMember = request.CallerId is { } callerId &&
                await context.GroupMemberships.AnyAsync(
                    m => m.GroupId == outcome.GroupId && m.UserId == callerId, cancellationToken);

            if (!isMember)
                throw new ForbiddenException("You are not a member of the group this market belongs to.");
        }

        var points = await context.OutcomePricePoints
            .Where(p => p.OutcomeId == request.OutcomeId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(request.Limit)
            .OrderBy(p => p.CreatedAt)
            .Select(p => new PricePointDto(p.Price, p.Volume, p.CreatedAt))
            .ToListAsync(cancellationToken);

        return points;
    }
}
