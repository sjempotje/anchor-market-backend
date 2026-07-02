using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Markets.DTOs;
using AnchorMarket.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Markets.Queries;

/// <summary>Query to retrieve markets. Pass <c>ActiveOnly = true</c> to exclude expired/resolved markets.</summary>
public record GetMarketsQuery(bool ActiveOnly = false) : IRequest<List<MarketDto>>;

public class GetMarketsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetMarketsQuery, List<MarketDto>>
{
    public async Task<List<MarketDto>> Handle(GetMarketsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Set<AnchorMarket.Domain.Entities.Market>()
            .Where(m => m.Scope == MarketScope.Public);

        var markets = await query.ToListAsync(cancellationToken);

        markets = markets.OrderByDescending(m => m.CreatedAt).ToList();

        if (request.ActiveOnly)
        {
            var now = DateTimeOffset.UtcNow;
            markets = markets
                .Where(m => m.Status == MarketStatus.Open && m.ResolutionDeadline > now)
                .ToList();
        }

        return markets
            .Select(m => new MarketDto(
                m.Id,
                m.Title,
                m.Description,
                m.ResolutionDeadline,
                m.Status,
                m.Scope,
                m.MarketType,
                m.CreatorId,
                m.GroupId,
                m.CategoryId,
                m.ImageUrl,
                m.BannerUrl,
                m.Thumbnail,
                m.Slug,
                m.Featured,
                m.TrendingScore,
                m.TotalBetAmount,
                m.BetCount,
                m.ResolutionSource,
                m.CreatedAt))
            .ToList();
    }
}
