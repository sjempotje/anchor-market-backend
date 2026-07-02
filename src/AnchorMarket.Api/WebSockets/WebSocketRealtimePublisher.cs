using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Realtime;

namespace AnchorMarket.Api.WebSockets;

/// <summary>
/// Publishes domain events onto the WebSocket backplane so subscribed clients see them live.
/// Message payloads use lowerCamelCase property names to match the client protocol.
/// </summary>
public sealed class WebSocketRealtimePublisher(WebSocketConnectionManager manager) : IRealtimePublisher
{
    public Task PublishPriceUpdateAsync(PriceUpdateEvent priceUpdate, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            type = "price-update",
            outcomeId = priceUpdate.OutcomeId,
            price = priceUpdate.Price,
            volume = priceUpdate.Volume,
            timestamp = priceUpdate.Timestamp,
        };
        return manager.BroadcastAsync(RealtimeTopics.Price(priceUpdate.OutcomeId), JsonSerializer.Serialize(payload), cancellationToken);
    }

    public Task PublishTradeAsync(TradeExecutedEvent trade, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            type = "trade-executed",
            marketId = trade.MarketId,
            outcomeId = trade.OutcomeId,
            price = trade.Price,
            shares = trade.Shares,
            timestamp = trade.Timestamp,
        };
        return manager.BroadcastAsync(RealtimeTopics.Trades(trade.MarketId), JsonSerializer.Serialize(payload), cancellationToken);
    }

    public Task PublishMarketResolvedAsync(MarketResolvedEvent marketResolved, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            type = "market-resolved",
            marketId = marketResolved.MarketId,
            winningOutcomeId = marketResolved.WinningOutcomeId,
            timestamp = marketResolved.Timestamp,
        };
        return manager.BroadcastAsync(RealtimeTopics.Market(marketResolved.MarketId), JsonSerializer.Serialize(payload), cancellationToken);
    }

    public Task PublishFeedUpdateAsync(FeedUpdateEvent feedUpdate, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            type = "feed-update",
            marketId = feedUpdate.MarketId,
            value = feedUpdate.Value,
            timestamp = feedUpdate.Timestamp,
        };
        return manager.BroadcastAsync(RealtimeTopics.Feed(feedUpdate.MarketId), JsonSerializer.Serialize(payload), cancellationToken);
    }
}
