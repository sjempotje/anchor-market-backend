using AnchorMarket.Application.Features.MarketResolutions.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;

namespace AnchorMarket.Application.Common.Mappings;

/// <summary>AutoMapper profile for mapping <see cref="MarketResolution"/> to its DTO.</summary>
public class MarketResolutionProfile : Profile
{
    /// <summary>Configures entity-to-DTO mappings.</summary>
    public MarketResolutionProfile()
    {
        CreateMap<MarketResolution, MarketResolutionDto>();
    }
}
