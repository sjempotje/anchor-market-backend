namespace AnchorMarket.Application.Common.Realtime;

/// <summary>Broadcast payload describing the latest traded price for an outcome.</summary>
/// <param name="OutcomeId">The outcome the price applies to.</param>
/// <param name="Price">The latest traded price.</param>
/// <param name="Volume">The shares exchanged in the originating trade.</param>
/// <param name="Timestamp">When the price was established.</param>
public record PriceUpdateEvent(
    Guid OutcomeId,
    decimal Price,
    decimal Volume,
    DateTimeOffset Timestamp);

/// <summary>Broadcast payload describing an executed trade.</summary>
/// <param name="MarketId">The market the trade occurred on.</param>
/// <param name="OutcomeId">The outcome that was traded.</param>
/// <param name="Price">The execution price.</param>
/// <param name="Shares">The number of shares exchanged.</param>
/// <param name="Timestamp">When the trade executed.</param>
public record TradeExecutedEvent(
    Guid MarketId,
    Guid OutcomeId,
    decimal Price,
    decimal Shares,
    DateTimeOffset Timestamp);

/// <summary>Broadcast payload describing a resolved market.</summary>
/// <param name="MarketId">The resolved market.</param>
/// <param name="GroupId">The owning group, when the market is group-scoped.</param>
/// <param name="WinningOutcomeId">The winning outcome.</param>
/// <param name="Timestamp">When the market was resolved.</param>
public record MarketResolvedEvent(
    Guid MarketId,
    Guid? GroupId,
    Guid WinningOutcomeId,
    DateTimeOffset Timestamp);
