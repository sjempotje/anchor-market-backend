using AnchorMarket.Application.Features.OrderBookHistory.DTOs;
using AnchorMarket.Application.Features.TradeFlow.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;

namespace AnchorMarket.Application.Common.Mappings;

/// <summary>AutoMapper profile for historical market data entities.</summary>
public class HistoricalDataProfile : Profile
{
    /// <summary>Configures entity-to-DTO mappings for snapshots.</summary>
    public HistoricalDataProfile()
    {
        CreateMap<OrderBookSnapshot, OrderBookSnapshotDto>();
        CreateMap<TradeFlowSnapshot, TradeFlowDto>();
    }
}
