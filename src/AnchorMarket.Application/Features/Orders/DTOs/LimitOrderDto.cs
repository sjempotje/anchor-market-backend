using AnchorMarket.Domain.Enums;

namespace AnchorMarket.Application.Features.Orders.DTOs;

/// <summary>Data transfer object for a limit order.</summary>
public record LimitOrderDto(
    Guid Id,
    Guid MarketId,
    Guid OutcomeId,
    Guid UserId,
    OrderSide Side,
    decimal Price,
    decimal Quantity,
    decimal FilledQuantity,
    decimal TotalCost,
    OrderType Type,
    OrderStatus Status,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

/// <summary>Data transfer object for a limit order with trade execution details.</summary>
public record LimitOrderDetailDto(
    Guid Id,
    Guid MarketId,
    string MarketTitle,
    Guid OutcomeId,
    string OutcomeTitle,
    Guid UserId,
    OrderSide Side,
    decimal Price,
    decimal Quantity,
    decimal FilledQuantity,
    decimal RemainingQuantity,
    decimal AverageFillPrice,
    decimal TotalCost,
    OrderType Type,
    OrderStatus Status,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyList<TradeExecutionDto> TradeExecutions);

/// <summary>Data transfer object for a trade execution.</summary>
public record TradeExecutionDto(
    Guid Id,
    Guid LimitOrderId,
    Guid BuyerOrderId,
    Guid SellerOrderId,
    Guid InitiatorUserId,
    decimal Shares,
    decimal ExecutedPrice,
    decimal TotalValue,
    DateTimeOffset CreatedAt);
