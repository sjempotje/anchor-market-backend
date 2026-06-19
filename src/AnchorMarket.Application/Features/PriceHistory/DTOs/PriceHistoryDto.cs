namespace AnchorMarket.Application.Features.PriceHistory.DTOs;

/// <summary>Data transfer object for a price history data point.</summary>
public record PriceHistoryDto(
    Guid Id,
    Guid OutcomeId,
    DateTimeOffset Timestamp,
    decimal Price,
    decimal Volume,
    decimal Liquidity);
