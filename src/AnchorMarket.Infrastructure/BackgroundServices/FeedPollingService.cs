using System.Collections.Concurrent;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Realtime;
using AnchorMarket.Domain.Entities;
using AnchorMarket.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AnchorMarket.Infrastructure.BackgroundServices;

/// <summary>
/// Polls every active external feed on its own interval, fetching the latest value through the
/// registered adapter, persisting the raw result, and broadcasting the value live. PostgreSQL is
/// the durable record of feed history.
/// </summary>
public class FeedPollingService(
    IServiceScopeFactory scopeFactory,
    IFeedAdapterFactory adapterFactory,
    IRealtimePublisher realtimePublisher,
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

        var broadcasts = new List<FeedUpdateEvent>();

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

            var receivedAt = DateTimeOffset.UtcNow;
            db.FeedResults.Add(FeedResult.Create(
                feed.Id, result.RawJson, result.ParsedValue, result.Status, result.ErrorMessage, receivedAt));

            if (result is { Status: FeedResultStatus.Success, ParsedValue: { } value })
                broadcasts.Add(new FeedUpdateEvent(feed.MarketId, feed.Id, value, receivedAt));
        }

        await db.SaveChangesAsync(cancellationToken);

        // Surface the latest feed values to subscribed clients after they're persisted.
        foreach (var update in broadcasts)
            await realtimePublisher.PublishFeedUpdateAsync(update, cancellationToken);
    }

    private bool IsDue(ExternalFeedRegistration feed, DateTimeOffset now)
    {
        if (!_lastPolled.TryGetValue(feed.Id, out var last))
            return true;
        var interval = Math.Max(feed.PollingIntervalMs, (int)TickInterval.TotalMilliseconds);
        return (now - last).TotalMilliseconds >= interval;
    }
}
