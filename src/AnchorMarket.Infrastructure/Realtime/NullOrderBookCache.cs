using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Enums;

namespace AnchorMarket.Infrastructure.Realtime;

/// <summary>No-op order book cache used when Redis is not configured (e.g. tests, single-node dev).</summary>
public class NullOrderBookCache : IOrderBookCache
{
    /// <inheritdoc />
    public Task AddRestingQuantityAsync(Guid outcomeId, OrderSide side, decimal price, decimal quantity, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc />
    public Task ReduceRestingQuantityAsync(Guid outcomeId, OrderSide side, decimal price, decimal quantity, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc />
    public Task SetLatestPriceAsync(Guid outcomeId, decimal price, decimal volume, DateTimeOffset timestamp, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc />
    public Task<OrderBookView> GetOrderBookAsync(Guid outcomeId, CancellationToken cancellationToken = default)
        => Task.FromResult(OrderBookView.Empty);

    /// <inheritdoc />
    public Task<LatestPrice?> GetLatestPriceAsync(Guid outcomeId, CancellationToken cancellationToken = default)
        => Task.FromResult<LatestPrice?>(null);

    /// <inheritdoc />
    public Task RebuildAsync(Guid outcomeId, IReadOnlyList<OrderBookLevel> bids, IReadOnlyList<OrderBookLevel> asks, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
