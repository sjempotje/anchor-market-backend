namespace AnchorMarket.Application.Features.Orders.DTOs;

public record OrderBookLevelDto(
    decimal Price,
    decimal TotalQuantity,
    int OrderCount);

public record OrderBookDto(
    Guid MarketId,
    string MarketTitle,
    Guid OutcomeId,
    string OutcomeTitle,
    IReadOnlyList<OrderBookLevelDto> Bids,
    IReadOnlyList<OrderBookLevelDto> Asks,
    decimal? BestBid,
    decimal? BestAsk,
    decimal Spread,
    DateTimeOffset UpdatedAt);

public record MarketPriceDto(
    Guid MarketId,
    string MarketTitle,
    Guid OutcomeId,
    string OutcomeTitle,
    decimal CurrentPrice,
    decimal PreviousPrice,
    decimal Change24h,
    decimal Change24hPercent,
    decimal High24h,
    decimal Low24h,
    decimal Volume24h,
    DateTimeOffset UpdatedAt);
