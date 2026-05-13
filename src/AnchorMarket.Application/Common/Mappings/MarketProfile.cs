using AnchorMarket.Application.Features.Markets.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;

namespace AnchorMarket.Application.Common.Mappings;

public class MarketProfile : Profile
{
    public MarketProfile()
    {
        CreateMap<Market, MarketDto>();
    }
}
