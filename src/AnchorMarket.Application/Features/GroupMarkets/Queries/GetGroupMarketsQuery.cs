using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Markets.DTOs;
using AnchorMarket.Domain.Enums;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.GroupMarkets.Queries;

/// <summary>Query to retrieve markets scoped to a group.</summary>
public record GetGroupMarketsQuery(Guid GroupId, Guid RequestingUserId) : IRequest<List<MarketDto>>;

/// <summary>Handles retrieving group markets.</summary>
public class GetGroupMarketsQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetGroupMarketsQuery, List<MarketDto>>
{
    /// <summary>Returns markets for the group if the user is a member.</summary>
    public async Task<List<MarketDto>> Handle(GetGroupMarketsQuery request, CancellationToken cancellationToken)
    {
        var isMember = await context.GroupMemberships
            .AnyAsync(m => m.GroupId == request.GroupId && m.UserId == request.RequestingUserId, cancellationToken);

        if (!isMember)
            return [];

        return await context.Markets
            .Where(m => m.GroupId == request.GroupId && m.Scope == MarketScope.Group)
            .ProjectTo<MarketDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
