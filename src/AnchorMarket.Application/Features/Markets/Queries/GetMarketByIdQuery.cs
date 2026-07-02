using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Markets.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Markets.Queries;

/// <summary>Query to retrieve a market by its ID.</summary>
/// <param name="Id">The market ID.</param>
/// <param name="CallerId">The authenticated caller, if any. Required to view group-scoped markets.</param>
public record GetMarketByIdQuery(Guid Id, Guid? CallerId = null) : IRequest<MarketDto?>;

/// <summary>Handles retrieving a market by ID, enforcing group membership for group-scoped markets.</summary>
public class GetMarketByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetMarketByIdQuery, MarketDto?>
{
    public async Task<MarketDto?> Handle(GetMarketByIdQuery request, CancellationToken cancellationToken)
    {
        var market = await context.Markets.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
        if (market is null) return null;

        if (market.GroupId.HasValue)
        {
            var isMember = request.CallerId is { } callerId &&
                await context.GroupMemberships.AnyAsync(
                    m => m.GroupId == market.GroupId && m.UserId == callerId, cancellationToken);

            if (!isMember)
                throw new ForbiddenException("You are not a member of the group this market belongs to.");
        }

        return mapper.Map<MarketDto>(market);
    }
}
