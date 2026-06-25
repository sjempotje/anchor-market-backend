using System.Text.Json;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Realtime;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AnchorMarket.Infrastructure.Redis;

/// <summary>
/// Publishes real-time events onto Redis pub/sub channels that the WebSocket layer subscribes to.
/// Publishing is best-effort: failures are logged and swallowed so a broadcast hiccup never aborts
/// the originating trade.
/// </summary>
public class RedisRealtimePublisher(IConnectionMultiplexer connection, ILogger<RedisRealtimePublisher> logger) : IRealtimePublisher
{
    private ISubscriber Subscriber => connection.GetSubscriber();

    /// <inheritdoc />
    public Task PublishPriceUpdateAsync(PriceUpdateEvent priceUpdate, CancellationToken cancellationToken = default)
        => PublishAsync(RedisKeys.Channels.PriceUpdates, priceUpdate);

    /// <inheritdoc />
    public Task PublishTradeAsync(TradeExecutedEvent trade, CancellationToken cancellationToken = default)
        => PublishAsync(RedisKeys.Channels.TradeExecutions, trade);

    /// <inheritdoc />
    public Task PublishMarketResolvedAsync(MarketResolvedEvent marketResolved, CancellationToken cancellationToken = default)
        => PublishAsync(RedisKeys.Channels.MarketResolved, marketResolved);

    private async Task PublishAsync<TEvent>(string channel, TEvent payload)
    {
        try
        {
            var json = JsonSerializer.Serialize(payload);
            await Subscriber.PublishAsync(RedisChannel.Literal(channel), json);
        }
        catch (RedisException ex)
        {
            logger.LogWarning(ex, "Failed to publish to real-time channel {Channel}.", channel);
        }
    }
}
