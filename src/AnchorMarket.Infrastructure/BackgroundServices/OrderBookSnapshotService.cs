using System.Text.Json;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using AnchorMarket.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AnchorMarket.Infrastructure.BackgroundServices;

/// <summary>
/// Periodically captures the live order book of every open market's outcomes into
/// <see cref="OrderBookSnapshot"/> rows for historical charting.
/// </summary>
public class OrderBookSnapshotService(
    IServiceScopeFactory scopeFactory,
    IOrderBookCache orderBookCache,
    ILogger<OrderBookSnapshotService> logger) : BackgroundService
{
    private static readonly TimeSpan CaptureInterval = TimeSpan.FromSeconds(5);

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("OrderBookSnapshotService started.");
        using var timer = new PeriodicTimer(CaptureInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await CaptureAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Order book snapshot capture failed.");
            }
        }
    }

    private async Task CaptureAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var outcomeIds = await db.Outcomes
            .Where(o => o.Market.Status == MarketStatus.Open)
            .Select(o => o.Id)
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var captured = 0;

        foreach (var outcomeId in outcomeIds)
        {
            var book = await orderBookCache.GetOrderBookAsync(outcomeId, cancellationToken);
            if (!book.HasLevels)
                continue;

            db.OrderBookSnapshots.Add(OrderBookSnapshot.Create(
                outcomeId, now,
                JsonSerializer.Serialize(book.Bids),
                JsonSerializer.Serialize(book.Asks),
                book.BestBid, book.BestAsk));
            captured++;
        }

        if (captured > 0)
            await db.SaveChangesAsync(cancellationToken);
    }
}
