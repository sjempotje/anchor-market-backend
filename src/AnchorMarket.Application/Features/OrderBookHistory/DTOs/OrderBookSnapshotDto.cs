namespace AnchorMarket.Application.Features.OrderBookHistory.DTOs;

/// <summary>Data transfer object for a historical order book snapshot.</summary>
public record OrderBookSnapshotDto(
    Guid Id,
    Guid OutcomeId,
    DateTimeOffset Timestamp,
    string Bids,
    string Asks,
    decimal? BestBid,
    decimal? BestAsk,
    decimal Spread);
