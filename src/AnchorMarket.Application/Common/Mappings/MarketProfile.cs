using AnchorMarket.Application.Features.Markets.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;

namespace AnchorMarket.Application.Common.Mappings;

/// <summary>AutoMapper profile for mapping between <see cref="Market"/> and <see cref="MarketDto"/>.</summary>
public class MarketProfile : Profile
{
    /// <summary>Configures entity-to-DTO mappings.</summary>
    public MarketProfile()
    {
        CreateMap<Market, MarketDto>()
            .ConstructUsing(src => new MarketDto(
                src.Id,
                src.Title,
                src.Description,
                src.ResolutionDeadline,
                src.Status,
                src.Scope,
                src.MarketType,
                src.CreatorId,
                src.GroupId,
                src.CategoryId,
                src.ImageUrl,
                src.BannerUrl,
                src.Thumbnail,
                src.Slug,
                src.Featured,
                src.TrendingScore,
                0m, // Volume24h
                0m, // Volume7d
                src.TotalBetAmount, // VolumeAllTime
                0m, // OpenInterest
                0m, // Liquidity
                src.BetCount, // TradesCount
                src.ResolutionSource,
                src.CreatedAt));

        CreateMap<Outcome, OutcomeDto>();
    }
}
