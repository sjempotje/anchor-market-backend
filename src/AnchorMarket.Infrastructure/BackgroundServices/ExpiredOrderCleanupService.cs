using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Orders.Commands;
using AnchorMarket.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AnchorMarket.Infrastructure.BackgroundServices;

/// <summary>
/// Periodically expires limit orders past their expiry, refunds the unfilled portion of buy orders,
/// and removes the remaining quantity from the live order book.
/// </summary>
public class ExpiredOrderCleanupService(
    IServiceScopeFactory scopeFactory,
    IOrderBookCache orderBookCache,
    ILogger<ExpiredOrderCleanupService> logger) : BackgroundService
{
    private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(1);

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ExpiredOrderCleanupService started.");
        using var timer = new PeriodicTimer(SweepInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await SweepAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Expired order sweep failed.");
            }
        }
    }

    private async Task SweepAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var walletService = scope.ServiceProvider.GetRequiredService<IWalletService>();

        var now = DateTimeOffset.UtcNow;
        var expired = await db.LimitOrders
            .Where(o => o.ExpiresAt != null
                        && o.ExpiresAt < now
                        && (o.Status == OrderStatus.Pending || o.Status == OrderStatus.PartiallyFilled))
            .ToListAsync(cancellationToken);

        if (expired.Count == 0)
            return;

        foreach (var order in expired)
        {
            order.MarkExpired();

            if (order.Side == OrderSide.Buy)
            {
                var unfilled = order.Quantity - order.FilledQuantity;
                var refund = unfilled * order.Price;
                if (refund > 0)
                    await walletService.CreditBalance(order.UserId, refund);
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        // Remove expired remainders from the live order book.
        foreach (var order in expired)
        {
            await orderBookCache.ReduceRestingQuantityAsync(
                order.OutcomeId, order.Side, order.Price, order.Quantity - order.FilledQuantity, cancellationToken);
        }

        logger.LogInformation("Expired {Count} order(s).", expired.Count);
    }
}
