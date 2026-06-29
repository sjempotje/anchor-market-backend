using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.ExternalFeeds.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.ExternalFeeds.Queries;

/// <summary>Query to retrieve recent raw results for a feed registration, newest first.</summary>
public record GetFeedResultsQuery(Guid FeedRegistrationId, int Limit = 100) : IRequest<List<FeedResultDto>>;

/// <summary>Handles retrieving recent feed results for a registration.</summary>
public class GetFeedResultsQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetFeedResultsQuery, List<FeedResultDto>>
{
    /// <summary>Returns the most recent feed results for the registration, capped by the requested limit.</summary>
    public async Task<List<FeedResultDto>> Handle(GetFeedResultsQuery request, CancellationToken cancellationToken)
    {
        var results = await context.FeedResults
            .Where(r => r.FeedRegistrationId == request.FeedRegistrationId)
            .ProjectTo<FeedResultDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        return [.. results
            .OrderByDescending(r => r.ReceivedAt)
            .Take(Math.Clamp(request.Limit, 1, 1000))];
    }
}
