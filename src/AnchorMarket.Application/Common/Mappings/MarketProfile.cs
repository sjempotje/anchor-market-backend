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
        CreateMap<Market, MarketDto>();
        CreateMap<Outcome, OutcomeDto>();
    }
}
