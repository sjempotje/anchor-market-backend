using AnchorMarket.Domain.Enums;

namespace AnchorMarket.Application.Common.Interfaces;

/// <summary>
/// Maintains the live, aggregated order book and latest price for outcomes in a fast cache.
/// The cache is a derived view; PostgreSQL remains the source of truth, so implementations must
/// tolerate transient failures without disrupting the calling transaction.
/// </summary>
public interface IOrderBookCache
{
    /// <summary>Adds resting quantity to a price level when an order rests on the book.</summary>
    /// <param name="outcomeId">The outcome the order is on.</param>
    /// <param name="side">Whether the order is a bid (buy) or ask (sell).</param>
    /// <param name="price">The price level.</param>
    /// <param name="quantity">The quantity to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddRestingQuantityAsync(Guid outcomeId, OrderSide side, decimal price, decimal quantity, CancellationToken cancellationToken = default);

    /// <summary>Removes resting quantity from a price level when an order fills, cancels, or expires.</summary>
    /// <param name="outcomeId">The outcome the order is on.</param>
    /// <param name="side">Whether the order is a bid (buy) or ask (sell).</param>
    /// <param name="price">The price level.</param>
    /// <param name="quantity">The quantity to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ReduceRestingQuantityAsync(Guid outcomeId, OrderSide side, decimal price, decimal quantity, CancellationToken cancellationToken = default);

    /// <summary>Records the latest traded price and volume for an outcome.</summary>
    /// <param name="outcomeId">The outcome the price applies to.</param>
    /// <param name="price">The latest traded price.</param>
    /// <param name="volume">The shares exchanged in the originating trade.</param>
    /// <param name="timestamp">When the price was established.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SetLatestPriceAsync(Guid outcomeId, decimal price, decimal volume, DateTimeOffset timestamp, CancellationToken cancellationToken = default);

    /// <summary>Reads the current aggregated order book for an outcome, sorted best-first on each side.</summary>
    /// <param name="outcomeId">The outcome to read.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The current book, or an empty view when unavailable.</returns>
    Task<OrderBookView> GetOrderBookAsync(Guid outcomeId, CancellationToken cancellationToken = default);

    /// <summary>Reads the latest traded price for an outcome.</summary>
    /// <param name="outcomeId">The outcome to read.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The latest price, or null when none has been recorded.</returns>
    Task<LatestPrice?> GetLatestPriceAsync(Guid outcomeId, CancellationToken cancellationToken = default);
}
