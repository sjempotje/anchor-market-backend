using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.ExternalFeeds.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.ExternalFeeds.Queries;

/// <summary>Query to retrieve all feed registrations for a market.</summary>
public record GetFeedsByMarketQuery(Guid MarketId) : IRequest<List<FeedRegistrationDto>>;

/// <summary>Handles retrieving feed registrations for a market.</summary>
public class GetFeedsByMarketQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetFeedsByMarketQuery, List<FeedRegistrationDto>>
{
    /// <summary>Returns the feed registrations for the specified market.</summary>
    public Task<List<FeedRegistrationDto>> Handle(GetFeedsByMarketQuery request, CancellationToken cancellationToken)
        => context.ExternalFeedRegistrations
            .Where(f => f.MarketId == request.MarketId)
            .ProjectTo<FeedRegistrationDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
}
