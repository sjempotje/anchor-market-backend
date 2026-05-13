using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Markets.DTOs;
using AnchorMarket.Domain.Enums;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.GroupMarkets.Queries;

/// <summary>
/// Returns markets scoped to the given group.
/// Visibility is enforced here!!!!! RequestingUserId must be a group member.
/// </summary>
public record GetGroupMarketsQuery(Guid GroupId, Guid RequestingUserId) : IRequest<List<MarketDto>>;

public class GetGroupMarketsQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetGroupMarketsQuery, List<MarketDto>>
{
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
