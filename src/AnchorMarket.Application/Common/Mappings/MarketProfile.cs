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
                src.TotalBetAmount,
                src.BetCount,
                src.ResolutionSource,
                src.CreatedAt));

        CreateMap<Outcome, OutcomeDto>()
            .ConstructUsing(src => new OutcomeDto(
                src.Id,
                src.MarketId,
                src.Title,
                src.ShortName,
                src.ImageUrl,
                src.Color,
                src.CountryCode,
                src.SortOrder,
                src.TotalBetAmount));
    }
}
