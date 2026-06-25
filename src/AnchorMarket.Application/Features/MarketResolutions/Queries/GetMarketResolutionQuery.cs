using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.MarketResolutions.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.MarketResolutions.Queries;

/// <summary>Query to retrieve a market's resolution (winner), if it has been resolved.</summary>
public record GetMarketResolutionQuery(Guid MarketId) : IRequest<MarketResolutionDto?>;

/// <summary>Handles retrieving a market's resolution.</summary>
public class GetMarketResolutionQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetMarketResolutionQuery, MarketResolutionDto?>
{
    /// <summary>Returns the market's resolution, or null if it is not yet resolved.</summary>
    public Task<MarketResolutionDto?> Handle(GetMarketResolutionQuery request, CancellationToken cancellationToken)
        => context.MarketResolutions
            .Where(r => r.MarketId == request.MarketId)
            .ProjectTo<MarketResolutionDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
}
