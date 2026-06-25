namespace AnchorMarket.Infrastructure.Redis;

/// <summary>Centralized Redis key patterns and pub/sub channel names for the real-time layer.</summary>
public static class RedisKeys
{
    /// <summary>Hash holding the latest price, volume, and timestamp for an outcome.</summary>
    public static string LatestPrice(Guid outcomeId) => $"price:{outcomeId}";

    /// <summary>Sorted set of active bid price levels for an outcome (score = price).</summary>
    public static string BidLevels(Guid outcomeId) => $"orderbook:{outcomeId}:bids";

    /// <summary>Sorted set of active ask price levels for an outcome (score = price).</summary>
    public static string AskLevels(Guid outcomeId) => $"orderbook:{outcomeId}:asks";

    /// <summary>Hash of resting quantity per bid price level for an outcome (field = price).</summary>
    public static string BidSizes(Guid outcomeId) => $"orderbook:{outcomeId}:bidsize";

    /// <summary>Hash of resting quantity per ask price level for an outcome (field = price).</summary>
    public static string AskSizes(Guid outcomeId) => $"orderbook:{outcomeId}:asksize";

    /// <summary>Field names within the latest-price hash.</summary>
    public static class PriceFields
    {
        /// <summary>The latest traded price.</summary>
        public const string Price = "price";
        /// <summary>The shares exchanged in the originating trade.</summary>
        public const string Volume = "volume";
        /// <summary>The Unix-millisecond timestamp the price was established.</summary>
        public const string Timestamp = "timestamp";
    }

    /// <summary>Pub/sub channels used as the WebSocket broadcast backplane.</summary>
    public static class Channels
    {
        /// <summary>Latest-price updates.</summary>
        public const string PriceUpdates = "ws:price-updates";
        /// <summary>Executed trades.</summary>
        public const string TradeExecutions = "ws:trade-executions";
        /// <summary>Order book level changes.</summary>
        public const string OrderBookChanges = "ws:orderbook-changes";
        /// <summary>Market resolution events.</summary>
        public const string MarketResolved = "ws:market-resolved";
    }
}
