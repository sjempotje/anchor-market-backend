using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using AnchorMarket.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AnchorMarket.Infrastructure.BackgroundServices;

/// <summary>
/// After a market resolves, downsamples its high-frequency price history into coarser time buckets
/// (per the feed's configured granularity) to bound long-term storage. Idempotent: each market is
/// processed once, tracked by <see cref="Market.PriceHistoryDownsampledAt"/>.
/// </summary>
public class MarketResolutionDownsampleService(
    IServiceScopeFactory scopeFactory,
    ILogger<MarketResolutionDownsampleService> logger) : BackgroundService
{
    private static readonly TimeSpan ScanInterval = TimeSpan.FromMinutes(5);
    private const int DefaultGranularitySeconds = 5;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("MarketResolutionDownsampleService started.");
        using var timer = new PeriodicTimer(ScanInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await DownsampleResolvedMarketsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Market resolution downsample scan failed.");
            }
        }
    }

    private async Task DownsampleResolvedMarketsAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var markets = await db.Markets
            .Where(m => m.Status == MarketStatus.Resolved && m.PriceHistoryDownsampledAt == null)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

        foreach (var marketId in markets)
            await DownsampleMarketAsync(db, marketId, cancellationToken);
    }

    private async Task DownsampleMarketAsync(IApplicationDbContext db, Guid marketId, CancellationToken cancellationToken)
    {
        var market = await db.Markets.FirstOrDefaultAsync(m => m.Id == marketId, cancellationToken);
        if (market is null)
            return;

        var feed = await db.ExternalFeedRegistrations
            .Where(f => f.MarketId == marketId)
            .Select(f => new { f.ResolutionGranularitySeconds, f.Config })
            .FirstOrDefaultAsync(cancellationToken);

        var granularity = feed?.ResolutionGranularitySeconds ?? DefaultGranularitySeconds;
        if (granularity < 1)
            granularity = DefaultGranularitySeconds;
        var strategy = DownsampleStrategy.FromConfig(feed?.Config);

        var outcomeIds = await db.Outcomes
            .Where(o => o.MarketId == marketId)
            .Select(o => o.Id)
            .ToListAsync(cancellationToken);

        foreach (var outcomeId in outcomeIds)
        {
            var points = await db.PriceHistory
                .Where(p => p.OutcomeId == outcomeId)
                .OrderBy(p => p.Timestamp)
                .ToListAsync(cancellationToken);

            if (points.Count == 0)
                continue;

            var aggregated = points
                .GroupBy(p => BucketStart(p.Timestamp, granularity))
                .Select(g =>
                {
                    var bucket = g.ToList();
                    return PriceHistory.Create(
                        outcomeId,
                        g.Key,
                        Aggregate(strategy.PriceField, bucket, p => p.Price),
                        Aggregate(strategy.VolumeField, bucket, p => p.Volume),
                        Aggregate(strategy.LiquidityField, bucket, p => p.Liquidity));
                })
                .ToList();

            // Only rewrite when downsampling actually reduces the row count.
            if (aggregated.Count >= points.Count)
                continue;

            db.PriceHistory.RemoveRange(points);
            await db.PriceHistory.AddRangeAsync(aggregated, cancellationToken);
        }

        market.MarkPriceHistoryDownsampled();
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Downsampled price history for resolved market {MarketId} (granularity {Granularity}s).", marketId, granularity);
    }

    private static DateTimeOffset BucketStart(DateTimeOffset timestamp, int granularitySeconds)
    {
        var seconds = timestamp.ToUnixTimeSeconds();
        var bucket = seconds - (seconds % granularitySeconds);
        return DateTimeOffset.FromUnixTimeSeconds(bucket);
    }

    /// <summary>Aggregates one field across a time bucket according to the configured strategy.</summary>
    private static decimal Aggregate(string field, List<Domain.Entities.PriceHistory> bucket, Func<Domain.Entities.PriceHistory, decimal> selector)
        => field switch
        {
            "last" => selector(bucket[^1]),
            "min" => bucket.Min(selector),
            "max" => bucket.Max(selector),
            "avg" => bucket.Average(selector),
            _ => selector(bucket[0]) // "first"
        };

    /// <summary>Per-field downsampling strategy, optionally supplied in the feed's Config JSON.</summary>
    private sealed record DownsampleStrategy(string PriceField, string VolumeField, string LiquidityField)
    {
        private static readonly string[] Allowed = ["first", "last", "min", "max", "avg"];
        private static readonly DownsampleStrategy Default = new("first", "max", "last");

        public static DownsampleStrategy FromConfig(string? config)
        {
            if (string.IsNullOrWhiteSpace(config))
                return Default;

            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(config);
                if (!doc.RootElement.TryGetProperty("DownsampleStrategy", out var s) || s.ValueKind != System.Text.Json.JsonValueKind.Object)
                    return Default;

                return new DownsampleStrategy(
                    Field(s, "PriceField", Default.PriceField),
                    Field(s, "VolumeField", Default.VolumeField),
                    Field(s, "LiquidityField", Default.LiquidityField));
            }
            catch (System.Text.Json.JsonException)
            {
                return Default;
            }
        }

        private static string Field(System.Text.Json.JsonElement strategy, string name, string fallback)
        {
            if (strategy.TryGetProperty(name, out var v) && v.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                var value = v.GetString()?.ToLowerInvariant();
                if (value is not null && Allowed.Contains(value))
                    return value;
            }
            return fallback;
        }
    }
}
