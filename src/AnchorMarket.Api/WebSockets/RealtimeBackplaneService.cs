using System.Text.Json;
using AnchorMarket.Application.Common.Realtime;
using AnchorMarket.Infrastructure.Redis;
using StackExchange.Redis;

namespace AnchorMarket.Api.WebSockets;

/// <summary>
/// Bridges the Redis pub/sub backplane to connected WebSocket clients: it subscribes to the
/// real-time channels and fans each event out to the connections subscribed to the matching topic.
/// Only registered when Redis is configured.
/// </summary>
public class RealtimeBackplaneService(
    IConnectionMultiplexer connection,
    WebSocketConnectionManager manager,
    ILogger<RealtimeBackplaneService> logger) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = connection.GetSubscriber();

        await subscriber.SubscribeAsync(
            RedisChannel.Literal(RedisKeys.Channels.PriceUpdates), (_, value) => OnPriceUpdate(value));
        await subscriber.SubscribeAsync(
            RedisChannel.Literal(RedisKeys.Channels.TradeExecutions), (_, value) => OnTradeExecuted(value));
        await subscriber.SubscribeAsync(
            RedisChannel.Literal(RedisKeys.Channels.MarketResolved), (_, value) => OnMarketResolved(value));
        await subscriber.SubscribeAsync(
            RedisChannel.Literal(RedisKeys.Channels.FeedUpdates), (_, value) => OnFeedUpdate(value));
        await subscriber.SubscribeAsync(
            RedisChannel.Literal(RedisKeys.Channels.OrderBookChanges), (_, value) => OnOrderBookUpdate(value));

        logger.LogInformation("RealtimeBackplaneService subscribed to real-time channels.");

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) { /* shutting down */ }

        await subscriber.UnsubscribeAllAsync();
    }

    private void OnPriceUpdate(RedisValue value)
    {
        var evt = Deserialize<PriceUpdateEvent>(value);
        if (evt is null)
            return;

        var message = JsonSerializer.Serialize(new
        {
            type = "price-update",
            outcomeId = evt.OutcomeId,
            price = evt.Price,
            volume = evt.Volume,
            timestamp = evt.Timestamp
        });

        _ = manager.BroadcastAsync(RealtimeTopics.Price(evt.OutcomeId), message);
    }

    private void OnTradeExecuted(RedisValue value)
    {
        var evt = Deserialize<TradeExecutedEvent>(value);
        if (evt is null)
            return;

        var message = JsonSerializer.Serialize(new
        {
            type = "trade-executed",
            marketId = evt.MarketId,
            outcomeId = evt.OutcomeId,
            price = evt.Price,
            shares = evt.Shares,
            timestamp = evt.Timestamp
        });

        _ = manager.BroadcastAsync(RealtimeTopics.Trades(evt.MarketId), message);
    }

    private void OnMarketResolved(RedisValue value)
    {
        var evt = Deserialize<MarketResolvedEvent>(value);
        if (evt is null)
            return;

        var message = JsonSerializer.Serialize(new
        {
            type = "market-resolved",
            marketId = evt.MarketId,
            winningOutcomeId = evt.WinningOutcomeId,
            timestamp = evt.Timestamp
        });

        _ = manager.BroadcastAsync(RealtimeTopics.Market(evt.MarketId), message);
        if (evt.GroupId is { } groupId)
            _ = manager.BroadcastAsync(RealtimeTopics.Group(groupId), message);
    }

    private void OnFeedUpdate(RedisValue value)
    {
        var evt = Deserialize<FeedUpdateEvent>(value);
        if (evt is null)
            return;

        var message = JsonSerializer.Serialize(new
        {
            type = "feed-update",
            marketId = evt.MarketId,
            feedRegistrationId = evt.FeedRegistrationId,
            value = evt.Value,
            timestamp = evt.Timestamp
        });

        _ = manager.BroadcastAsync(RealtimeTopics.Feed(evt.MarketId), message);
    }

    private void OnOrderBookUpdate(RedisValue value)
    {
        var evt = Deserialize<OrderBookUpdateEvent>(value);
        if (evt is null)
            return;

        var message = JsonSerializer.Serialize(new
        {
            type = "orderbook-update",
            outcomeId = evt.OutcomeId,
            bids = evt.Bids,
            asks = evt.Asks,
            timestamp = evt.Timestamp
        });

        _ = manager.BroadcastAsync(RealtimeTopics.OrderBook(evt.OutcomeId), message);
    }

    private T? Deserialize<T>(RedisValue value)
    {
        try
        {
            return JsonSerializer.Deserialize<T>((string)value!);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failed to deserialize a real-time event of type {Type}.", typeof(T).Name);
            return default;
        }
    }
}
