using AnchorMarket.Application.Features.PriceHistory.DTOs;
using AutoMapper;

namespace AnchorMarket.Application.Common.Mappings;

/// <summary>AutoMapper profile for mapping between <see cref="PriceHistory"/> and <see cref="PriceHistoryDto"/>.</summary>
public class PriceHistoryProfile : Profile
{
    /// <summary>Configures entity-to-DTO mappings.</summary>
    public PriceHistoryProfile()
    {
        CreateMap<AnchorMarket.Domain.Entities.PriceHistory, PriceHistoryDto>();
    }
}
