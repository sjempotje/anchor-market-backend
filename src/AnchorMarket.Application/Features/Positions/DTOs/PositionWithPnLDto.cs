namespace AnchorMarket.Application.Features.Positions.DTOs;

public record PositionWithPnLDto(
    Guid Id,
    Guid UserId,
    Guid MarketId,
    string MarketTitle,
    Guid OutcomeId,
    string OutcomeTitle,
    decimal Amount,
    decimal Shares,
    decimal EntryPrice,
    decimal FairValueAtEntry,
    decimal CurrentFairValue,
    decimal UnrealizedPnL,
    decimal ReturnOnInvestment,
    decimal CurrentPrice,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
