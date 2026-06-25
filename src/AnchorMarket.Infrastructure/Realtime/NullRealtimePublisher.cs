using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Realtime;

namespace AnchorMarket.Infrastructure.Realtime;

/// <summary>No-op real-time publisher used when Redis is not configured (e.g. tests, single-node dev).</summary>
public class NullRealtimePublisher : IRealtimePublisher
{
    /// <inheritdoc />
    public Task PublishPriceUpdateAsync(PriceUpdateEvent priceUpdate, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc />
    public Task PublishTradeAsync(TradeExecutedEvent trade, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
