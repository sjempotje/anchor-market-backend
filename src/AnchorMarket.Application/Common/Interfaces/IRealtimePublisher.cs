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
}
