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

        var granularity = await db.ExternalFeedRegistrations
            .Where(f => f.MarketId == marketId)
            .Select(f => (int?)f.ResolutionGranularitySeconds)
            .FirstOrDefaultAsync(cancellationToken) ?? DefaultGranularitySeconds;
        if (granularity < 1)
            granularity = DefaultGranularitySeconds;

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
                .Select(g => PriceHistory.Create(
                    outcomeId,
                    g.Key,
                    g.First().Price,        // first price in the bucket
                    g.Max(p => p.Volume),   // peak volume
                    g.Last().Liquidity))    // last liquidity
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
}
