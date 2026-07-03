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
        var betsByOutcome = await db.Positions
            .GroupBy(p => p.OutcomeId)
            .Select(g => new { OutcomeId = g.Key, TotalBetAmount = g.Sum(p => p.Amount) })
            .ToDictionaryAsync(x => x.OutcomeId, x => x.TotalBetAmount, cancellationToken);

        if (betsByOutcome.Count == 0)
            return;

        var outcomes = await db.Outcomes
            .Where(o => betsByOutcome.Keys.Contains(o.Id))
            .ToListAsync(cancellationToken);

        foreach (var outcome in outcomes)
        {
            var totalBetAmount = betsByOutcome.GetValueOrDefault(outcome.Id);
            outcome.UpdateStats(totalBetAmount);
        }
    }

    private static async Task UpdateMarketStatsAsync(IApplicationDbContext db, DateTimeOffset cutoff24h, DateTimeOffset cutoff7d, CancellationToken cancellationToken)
    {
        var betsByMarket = await db.Positions
            .Join(db.Outcomes, p => p.OutcomeId, o => o.Id, (p, o) => new { o.MarketId, p.Amount })
            .GroupBy(x => x.MarketId)
            .Select(g => new { MarketId = g.Key, TotalBetAmount = g.Sum(x => x.Amount), BetCount = g.Count() })
            .ToListAsync(cancellationToken);

        if (betsByMarket.Count == 0)
            return;

        var markets = await db.Markets
            .Where(m => betsByMarket.Select(b => b.MarketId).Contains(m.Id))
            .ToListAsync(cancellationToken);

        foreach (var market in markets)
        {
            var bets = betsByMarket.FirstOrDefault(x => x.MarketId == market.Id);
            if (bets != null)
            {
                market.UpdateStats(bets.TotalBetAmount, bets.BetCount);
            }
        }
    }
}
