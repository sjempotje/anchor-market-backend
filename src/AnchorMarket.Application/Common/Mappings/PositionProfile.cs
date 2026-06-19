using AnchorMarket.Application.Features.Positions.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;

namespace AnchorMarket.Application.Common.Mappings;

/// <summary>AutoMapper profile for mapping between <see cref="Position"/> and <see cref="PositionDto"/>.</summary>
public class PositionProfile : Profile
{
    /// <summary>Configures entity-to-DTO mappings.</summary>
    public PositionProfile()
    {
        CreateMap<Position, PositionDto>();
        
        CreateMap<Position, PositionWithPnLDto>()
            .ForMember(dest => dest.UnrealizedPnL, opt => opt.MapFrom(src => src.CalculateUnrealizedPnL()))
            .ForMember(dest => dest.ReturnOnInvestment, opt => opt.MapFrom(src => src.CalculateReturnOnInvestment()));
    }
}
