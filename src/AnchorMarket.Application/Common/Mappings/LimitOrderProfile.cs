using AnchorMarket.Application.Features.Orders.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;

namespace AnchorMarket.Application.Common.Mappings;

/// <summary>AutoMapper profile for mapping between <see cref="LimitOrder"/> and <see cref="LimitOrderDto"/>.</summary>
public class LimitOrderProfile : Profile
{
    /// <summary>Configures entity-to-DTO mappings.</summary>
    public LimitOrderProfile()
    {
        CreateMap<LimitOrder, LimitOrderDto>();
        
        CreateMap<LimitOrder, LimitOrderDetailDto>()
            .ForMember(dest => dest.RemainingQuantity, opt => opt.MapFrom(src => src.Quantity - src.FilledQuantity))
            .ForMember(dest => dest.AverageFillPrice, opt => opt.MapFrom(src => 
                src.FilledQuantity > 0 ? src.TotalCost / src.FilledQuantity : 0m))
            .ForMember(dest => dest.MarketTitle, opt => opt.MapFrom(src => src.Market.Title))
            .ForMember(dest => dest.OutcomeTitle, opt => opt.MapFrom(src => src.Outcome.Title))
            .ForMember(dest => dest.TradeExecutions, opt => opt.MapFrom(src => src.TradeExecutions));

        CreateMap<TradeExecution, TradeExecutionDto>();
    }
}
