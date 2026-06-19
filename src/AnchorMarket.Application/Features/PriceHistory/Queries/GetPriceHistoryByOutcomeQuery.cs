using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.PriceHistory.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.PriceHistory.Queries;

/// <summary>Query to retrieve price history for a market outcome.</summary>
public record GetPriceHistoryByOutcomeQuery(Guid OutcomeId) : IRequest<List<PriceHistoryDto>>;

/// <summary>Handles retrieving price history for an outcome.</summary>
public class GetPriceHistoryByOutcomeQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetPriceHistoryByOutcomeQuery, List<PriceHistoryDto>>
{
    /// <summary>Returns the price history for the specified outcome, ordered by timestamp.</summary>
    public Task<List<PriceHistoryDto>> Handle(GetPriceHistoryByOutcomeQuery request, CancellationToken cancellationToken)
        => context.PriceHistory
            .Where(p => p.OutcomeId == request.OutcomeId)
            .OrderBy(p => p.Timestamp)
            .ProjectTo<PriceHistoryDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
}
