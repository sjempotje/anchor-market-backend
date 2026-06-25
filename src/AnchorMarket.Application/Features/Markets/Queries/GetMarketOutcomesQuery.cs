using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Markets.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Markets.Queries;

/// <summary>Query to retrieve the outcomes of a market, ordered for display.</summary>
public record GetMarketOutcomesQuery(Guid MarketId) : IRequest<List<OutcomeDto>>;

/// <summary>Handles retrieving a market's outcomes.</summary>
public class GetMarketOutcomesQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetMarketOutcomesQuery, List<OutcomeDto>>
{
    /// <summary>Returns the market's outcomes ordered by sort order.</summary>
    public Task<List<OutcomeDto>> Handle(GetMarketOutcomesQuery request, CancellationToken cancellationToken)
        => context.Outcomes
            .Where(o => o.MarketId == request.MarketId)
            .OrderBy(o => o.SortOrder)
            .ProjectTo<OutcomeDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
}
