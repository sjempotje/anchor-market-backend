using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Markets.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.GroupMarkets.Queries;

/// <summary>Query to retrieve a group market by its ID, enforcing group membership.</summary>
public record GetGroupMarketByIdQuery(Guid Id, Guid RequestingUserId) : IRequest<MarketDto?>;

/// <summary>Handles retrieving a group market by ID.</summary>
public class GetGroupMarketByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetGroupMarketByIdQuery, MarketDto?>
{
    public async Task<MarketDto?> Handle(GetGroupMarketByIdQuery request, CancellationToken cancellationToken)
    {
        var market = await context.Markets
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (market is null)
            return null;

        if (market.GroupId.HasValue)
        {
            var isMember = await context.GroupMemberships
                .AnyAsync(m => m.GroupId == market.GroupId && m.UserId == request.RequestingUserId, cancellationToken);

            if (!isMember)
                throw new ForbiddenException("You are not a member of the group this market belongs to.");
        }

        return mapper.Map<MarketDto>(market);
    }
}
