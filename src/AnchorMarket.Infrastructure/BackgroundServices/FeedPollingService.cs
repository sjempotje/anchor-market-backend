using System.Collections.Concurrent;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AnchorMarket.Infrastructure.BackgroundServices;

/// <summary>
/// Polls every active external feed on its own interval, fetching the latest value through the
/// registered adapter and persisting the raw result. This is the durable record of feed history;
/// PostgreSQL is the source of truth.
/// </summary>
public class FeedPollingService(
    IServiceScopeFactory scopeFactory,
    IFeedAdapterFactory adapterFactory,
    ILogger<FeedPollingService> logger) : BackgroundService
{
    /// <summary>How often the service wakes to check which feeds are due.</summary>
    private static readonly TimeSpan TickInterval = TimeSpan.FromMilliseconds(500);

    private readonly ConcurrentDictionary<Guid, DateTimeOffset> _lastPolled = new();

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("FeedPollingService started.");
        using var timer = new PeriodicTimer(TickInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await PollDueFeedsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Feed polling tick failed.");
            }
        }
    }

    private async Task PollDueFeedsAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var activeFeeds = await db.ExternalFeedRegistrations
            .Where(f => f.IsActive)
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var due = activeFeeds.Where(f => IsDue(f, now)).ToList();
        if (due.Count == 0)
            return;

        foreach (var feed in due)
        {
            if (!adapterFactory.Supports(feed.AdapterType))
            {
                logger.LogWarning("Feed {FeedId} references unknown adapter '{AdapterType}'; skipping.", feed.Id, feed.AdapterType);
                continue;
            }

            _lastPolled[feed.Id] = now;
            var adapter = adapterFactory.Resolve(feed.AdapterType);
            var result = await adapter.FetchAsync(feed, cancellationToken);

            db.FeedResults.Add(FeedResult.Create(
                feed.Id, result.RawJson, result.ParsedValue, result.Status, result.ErrorMessage, DateTimeOffset.UtcNow));
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private bool IsDue(ExternalFeedRegistration feed, DateTimeOffset now)
    {
        if (!_lastPolled.TryGetValue(feed.Id, out var last))
            return true;
        var interval = Math.Max(feed.PollingIntervalMs, (int)TickInterval.TotalMilliseconds);
        return (now - last).TotalMilliseconds >= interval;
    }
}
