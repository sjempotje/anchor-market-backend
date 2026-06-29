using AnchorMarket.Application.Common.Realtime;

namespace AnchorMarket.Application.Common.Interfaces;

/// <summary>
/// Publishes domain events to the real-time backplane so connected clients can be notified.
/// Implementations must tolerate transient failures without disrupting the calling transaction.
/// </summary>
public interface IRealtimePublisher
{
    /// <summary>Publishes the latest price for an outcome.</summary>
    /// <param name="priceUpdate">The price update payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishPriceUpdateAsync(PriceUpdateEvent priceUpdate, CancellationToken cancellationToken = default);

    /// <summary>Publishes an executed trade.</summary>
    /// <param name="trade">The trade payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishTradeAsync(TradeExecutedEvent trade, CancellationToken cancellationToken = default);

    /// <summary>Publishes a market resolution.</summary>
    /// <param name="marketResolved">The resolution payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishMarketResolvedAsync(MarketResolvedEvent marketResolved, CancellationToken cancellationToken = default);

    /// <summary>Publishes the latest external feed value for a market.</summary>
    /// <param name="feedUpdate">The feed update payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishFeedUpdateAsync(FeedUpdateEvent feedUpdate, CancellationToken cancellationToken = default);

    /// <summary>Publishes the current order book for an outcome.</summary>
    /// <param name="orderBookUpdate">The order book payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishOrderBookUpdateAsync(OrderBookUpdateEvent orderBookUpdate, CancellationToken cancellationToken = default);
}
