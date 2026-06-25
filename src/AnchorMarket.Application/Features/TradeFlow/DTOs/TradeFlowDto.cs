namespace AnchorMarket.Application.Features.TradeFlow.DTOs;

/// <summary>Data transfer object for a trade flow snapshot.</summary>
public record TradeFlowDto(
    Guid Id,
    Guid MarketId,
    Guid OutcomeId,
    DateTimeOffset Timestamp,
    decimal ExecutedPrice,
    decimal Shares,
    Guid BuyerOrderId,
    Guid SellerOrderId,
    decimal BidDepthAtTrade,
    decimal AskDepthAtTrade);
