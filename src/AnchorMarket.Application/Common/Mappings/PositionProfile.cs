using AnchorMarket.Application.Features.Positions.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;

namespace AnchorMarket.Application.Common.Mappings;

public class PositionProfile : Profile
{
    public PositionProfile()
    {
        CreateMap<Position, PositionDto>();
    }
}
