using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Markets.DTOs;
using AnchorMarket.Domain.Enums;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Markets.Queries;

/// <summary>Query to retrieve markets. Pass <c>ActiveOnly = true</c> to exclude expired/resolved markets.</summary>
public record GetMarketsQuery(bool ActiveOnly = false) : IRequest<List<MarketDto>>;

public class GetMarketsQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetMarketsQuery, List<MarketDto>>
{
    public Task<List<MarketDto>> Handle(GetMarketsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Set<AnchorMarket.Domain.Entities.Market>()
            .Where(m => m.Scope == MarketScope.Public);

        if (request.ActiveOnly)
        {
            var now = DateTimeOffset.UtcNow;
            query = query.Where(m => m.Status == MarketStatus.Open && m.ResolutionDeadline > now);
        }

        return query
            .OrderByDescending(m => m.CreatedAt)
            .ProjectTo<MarketDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
