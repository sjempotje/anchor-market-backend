using AnchorMarket.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AnchorMarket.Infrastructure.BackgroundServices;

/// <summary>
/// Periodically aggregates trade executions and positions to refresh the denormalized volume,
/// open-interest, and trade-count statistics on outcomes and markets.
/// </summary>
public class VolumeStatsUpdaterService(
    IServiceScopeFactory scopeFactory,
    ILogger<VolumeStatsUpdaterService> logger) : BackgroundService
{
    private static readonly TimeSpan UpdateInterval = TimeSpan.FromSeconds(30);

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("VolumeStatsUpdaterService started.");
        using var timer = new PeriodicTimer(UpdateInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await UpdateStatsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Volume stats update failed.");
            }
        }
    }

    private async Task UpdateStatsAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var now = DateTimeOffset.UtcNow;
        var cutoff24h = now.AddDays(-1);
        var cutoff7d = now.AddDays(-7);

        await UpdateOutcomeStatsAsync(db, cancellationToken);
        await UpdateMarketStatsAsync(db, cutoff24h, cutoff7d, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task UpdateOutcomeStatsAsync(IApplicationDbContext db, CancellationToken cancellationToken)
    {
        var volumeByOutcome = await db.TradeExecutions
            .GroupBy(t => t.OutcomeId)
            .Select(g => new { OutcomeId = g.Key, Volume = g.Sum(t => t.TotalValue) })
            .ToDictionaryAsync(x => x.OutcomeId, x => x.Volume, cancellationToken);

        var openInterestByOutcome = await db.Positions
            .GroupBy(p => p.OutcomeId)
            .Select(g => new { OutcomeId = g.Key, OpenInterest = g.Sum(p => p.Shares) })
            .ToDictionaryAsync(x => x.OutcomeId, x => x.OpenInterest, cancellationToken);

        var outcomeIds = volumeByOutcome.Keys.Union(openInterestByOutcome.Keys).ToList();
        if (outcomeIds.Count == 0)
            return;

        var outcomes = await db.Outcomes
            .Where(o => outcomeIds.Contains(o.Id))
            .ToListAsync(cancellationToken);

        foreach (var outcome in outcomes)
        {
            var volume = volumeByOutcome.GetValueOrDefault(outcome.Id);
            var openInterest = openInterestByOutcome.GetValueOrDefault(outcome.Id);
            outcome.UpdateStats(volume, openInterest);
        }
    }

    private static async Task UpdateMarketStatsAsync(IApplicationDbContext db, DateTimeOffset cutoff24h, DateTimeOffset cutoff7d, CancellationToken cancellationToken)
    {
        var allTime = await db.TradeExecutions
            .GroupBy(t => t.MarketId)
            .Select(g => new { MarketId = g.Key, Volume = g.Sum(t => t.TotalValue), Count = g.Count() })
            .ToListAsync(cancellationToken);

        if (allTime.Count == 0)
            return;

        var volume24h = await db.TradeExecutions
            .Where(t => t.CreatedAt >= cutoff24h)
            .GroupBy(t => t.MarketId)
            .Select(g => new { MarketId = g.Key, Volume = g.Sum(t => t.TotalValue) })
            .ToDictionaryAsync(x => x.MarketId, x => x.Volume, cancellationToken);

        var volume7d = await db.TradeExecutions
            .Where(t => t.CreatedAt >= cutoff7d)
            .GroupBy(t => t.MarketId)
            .Select(g => new { MarketId = g.Key, Volume = g.Sum(t => t.TotalValue) })
            .ToDictionaryAsync(x => x.MarketId, x => x.Volume, cancellationToken);

        // Open interest per market = total shares held across its outcomes.
        var openInterestByMarket = await db.Positions
            .Join(db.Outcomes, p => p.OutcomeId, o => o.Id, (p, o) => new { o.MarketId, p.Shares })
            .GroupBy(x => x.MarketId)
            .Select(g => new { MarketId = g.Key, OpenInterest = g.Sum(x => x.Shares) })
            .ToDictionaryAsync(x => x.MarketId, x => x.OpenInterest, cancellationToken);

        var marketIds = allTime.Select(x => x.MarketId).ToList();
        var markets = await db.Markets
            .Where(m => marketIds.Contains(m.Id))
            .ToListAsync(cancellationToken);

        foreach (var market in markets)
        {
            var totals = allTime.First(x => x.MarketId == market.Id);
            market.UpdateStats(
                volume24h.GetValueOrDefault(market.Id),
                volume7d.GetValueOrDefault(market.Id),
                totals.Volume,
                openInterestByMarket.GetValueOrDefault(market.Id),
                market.Liquidity, // preserve liquidity, maintained elsewhere
                totals.Count);
        }
    }
}
