using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using AnchorMarket.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AnchorMarket.Infrastructure.BackgroundServices;

/// <summary>
/// Periodically records a <see cref="PriceHistory"/> point for each open market's outcomes, using the
/// latest traded price (falling back to the order book mid-price), enriched with current book depth.
/// </summary>
public class PriceSnapshotService(
    IServiceScopeFactory scopeFactory,
    IOrderBookCache orderBookCache,
    ILogger<PriceSnapshotService> logger) : BackgroundService
{
    private static readonly TimeSpan SnapshotInterval = TimeSpan.FromSeconds(1);

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("PriceSnapshotService started.");
        using var timer = new PeriodicTimer(SnapshotInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await SnapshotAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Price snapshot tick failed.");
            }
        }
    }

    private async Task SnapshotAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var outcomeIds = await db.Outcomes
            .Where(o => o.Market.Status == MarketStatus.Open)
            .Select(o => o.Id)
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var written = 0;

        foreach (var outcomeId in outcomeIds)
        {
            var latest = await orderBookCache.GetLatestPriceAsync(outcomeId, cancellationToken);
            var book = await orderBookCache.GetOrderBookAsync(outcomeId, cancellationToken);

            var price = latest?.Price ?? MidPrice(book);
            if (price is null)
                continue;

            var liquidity = book.Bids.Sum(b => b.Quantity) + book.Asks.Sum(a => a.Quantity);
            db.PriceHistory.Add(PriceHistory.Create(outcomeId, now, price.Value, latest?.Volume ?? 0, liquidity));
            written++;
        }

        if (written > 0)
            await db.SaveChangesAsync(cancellationToken);
    }

    private static decimal? MidPrice(OrderBookView book)
        => book.BestBid.HasValue && book.BestAsk.HasValue
            ? (book.BestBid.Value + book.BestAsk.Value) / 2
            : null;
}
