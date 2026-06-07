using AnchorMarket.Application.Features.Positions.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;

namespace AnchorMarket.Application.Common.Mappings;

public class PositionProfile : Profile
{
    public PositionProfile()
    {
        CreateMap<Position, PositionDto>();
        
        CreateMap<Position, PositionWithPnLDto>()
            .ForMember(dest => dest.UnrealizedPnL, opt => opt.MapFrom(src => src.CalculateUnrealizedPnL()))
            .ForMember(dest => dest.ReturnOnInvestment, opt => opt.MapFrom(src => src.CalculateReturnOnInvestment()));
    }
}
