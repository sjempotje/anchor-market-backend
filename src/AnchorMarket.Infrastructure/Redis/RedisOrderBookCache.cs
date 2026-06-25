using System.Globalization;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Enums;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AnchorMarket.Infrastructure.Redis;

/// <summary>
/// Redis-backed live order book. Each side is held as a sorted set of price levels (score = price)
/// plus a hash of resting quantity per level, enabling fast best-bid/ask reads and atomic level
/// deltas. All operations are best-effort: Redis failures are logged and swallowed so they never
/// abort a trade, since PostgreSQL is the source of truth.
/// </summary>
public class RedisOrderBookCache(IConnectionMultiplexer connection, ILogger<RedisOrderBookCache> logger) : IOrderBookCache
{
    private const decimal QuantityEpsilon = 0.0000001m;

    private IDatabase Db => connection.GetDatabase();

    /// <inheritdoc />
    public async Task AddRestingQuantityAsync(Guid outcomeId, OrderSide side, decimal price, decimal quantity, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
            return;

        var (levelsKey, sizesKey) = KeysFor(outcomeId, side);
        var member = Format(price);

        try
        {
            var db = Db;
            await db.SortedSetAddAsync(levelsKey, member, (double)price);
            await db.HashIncrementAsync(sizesKey, member, (double)quantity);
        }
        catch (RedisException ex)
        {
            logger.LogWarning(ex, "Failed to add resting quantity to order book cache for outcome {OutcomeId}.", outcomeId);
        }
    }

    /// <inheritdoc />
    public async Task ReduceRestingQuantityAsync(Guid outcomeId, OrderSide side, decimal price, decimal quantity, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
            return;

        var (levelsKey, sizesKey) = KeysFor(outcomeId, side);
        var member = Format(price);

        try
        {
            var db = Db;
            var remaining = await db.HashDecrementAsync(sizesKey, member, (double)quantity);
            if (remaining <= (double)QuantityEpsilon)
            {
                await db.HashDeleteAsync(sizesKey, member);
                await db.SortedSetRemoveAsync(levelsKey, member);
            }
        }
        catch (RedisException ex)
        {
            logger.LogWarning(ex, "Failed to reduce resting quantity in order book cache for outcome {OutcomeId}.", outcomeId);
        }
    }

    /// <inheritdoc />
    public async Task SetLatestPriceAsync(Guid outcomeId, decimal price, decimal volume, DateTimeOffset timestamp, CancellationToken cancellationToken = default)
    {
        try
        {
            await Db.HashSetAsync(RedisKeys.LatestPrice(outcomeId),
            [
                new HashEntry(RedisKeys.PriceFields.Price, Format(price)),
                new HashEntry(RedisKeys.PriceFields.Volume, Format(volume)),
                new HashEntry(RedisKeys.PriceFields.Timestamp, timestamp.ToUnixTimeMilliseconds())
            ]);
        }
        catch (RedisException ex)
        {
            logger.LogWarning(ex, "Failed to set latest price in cache for outcome {OutcomeId}.", outcomeId);
        }
    }

    /// <inheritdoc />
    public async Task<OrderBookView> GetOrderBookAsync(Guid outcomeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = Db;
            var bids = await ReadSideAsync(db, RedisKeys.BidLevels(outcomeId), RedisKeys.BidSizes(outcomeId), Order.Descending);
            var asks = await ReadSideAsync(db, RedisKeys.AskLevels(outcomeId), RedisKeys.AskSizes(outcomeId), Order.Ascending);
            return new OrderBookView(bids, asks);
        }
        catch (RedisException ex)
        {
            logger.LogWarning(ex, "Failed to read order book from cache for outcome {OutcomeId}.", outcomeId);
            return OrderBookView.Empty;
        }
    }

    /// <inheritdoc />
    public async Task<LatestPrice?> GetLatestPriceAsync(Guid outcomeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var entries = await Db.HashGetAllAsync(RedisKeys.LatestPrice(outcomeId));
            if (entries.Length == 0)
                return null;

            var map = entries.ToDictionary(e => (string)e.Name!, e => e.Value);
            if (!map.TryGetValue(RedisKeys.PriceFields.Price, out var priceValue)
                || !decimal.TryParse((string?)priceValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                return null;

            decimal volume = 0;
            if (map.TryGetValue(RedisKeys.PriceFields.Volume, out var volumeValue))
                decimal.TryParse((string?)volumeValue, NumberStyles.Any, CultureInfo.InvariantCulture, out volume);

            var timestamp = DateTimeOffset.UtcNow;
            if (map.TryGetValue(RedisKeys.PriceFields.Timestamp, out var tsValue) && tsValue.TryParse(out long unixMs))
                timestamp = DateTimeOffset.FromUnixTimeMilliseconds(unixMs);

            return new LatestPrice(price, volume, timestamp);
        }
        catch (RedisException ex)
        {
            logger.LogWarning(ex, "Failed to read latest price from cache for outcome {OutcomeId}.", outcomeId);
            return null;
        }
    }

    private static async Task<IReadOnlyList<OrderBookLevel>> ReadSideAsync(IDatabase db, string levelsKey, string sizesKey, Order order)
    {
        var members = await db.SortedSetRangeByRankAsync(levelsKey, order: order);
        if (members.Length == 0)
            return [];

        var sizes = await db.HashGetAllAsync(sizesKey);
        var sizeByPrice = sizes.ToDictionary(h => (string)h.Name!, h => h.Value);

        var levels = new List<OrderBookLevel>(members.Length);
        foreach (var member in members)
        {
            var priceText = (string)member!;
            if (!decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                continue;
            if (!sizeByPrice.TryGetValue(priceText, out var sizeValue)
                || !decimal.TryParse((string?)sizeValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var quantity)
                || quantity <= 0)
                continue;
            levels.Add(new OrderBookLevel(price, quantity));
        }

        return levels;
    }

    private static (string LevelsKey, string SizesKey) KeysFor(Guid outcomeId, OrderSide side)
        => side == OrderSide.Buy
            ? (RedisKeys.BidLevels(outcomeId), RedisKeys.BidSizes(outcomeId))
            : (RedisKeys.AskLevels(outcomeId), RedisKeys.AskSizes(outcomeId));

    private static string Format(decimal value) => value.ToString(CultureInfo.InvariantCulture);
}
