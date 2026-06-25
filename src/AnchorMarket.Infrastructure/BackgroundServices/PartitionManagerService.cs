using System.Globalization;
using AnchorMarket.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AnchorMarket.Infrastructure.BackgroundServices;

/// <summary>
/// Ensures the current and next month range partitions exist for the partitioned time-series tables,
/// so incoming rows land in a monthly partition rather than the catch-all default. Runs at startup
/// and daily. PostgreSQL only.
/// </summary>
public class PartitionManagerService(
    IServiceScopeFactory scopeFactory,
    ILogger<PartitionManagerService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);
    private static readonly string[] PartitionedTables = ["PriceHistory", "OrderBookSnapshots", "FeedResults"];

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("PartitionManagerService started.");
        await EnsurePartitionsAsync(stoppingToken);

        using var timer = new PeriodicTimer(Interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await EnsurePartitionsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Partition provisioning failed.");
            }
        }
    }

    private async Task EnsurePartitionsAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        // Partitioning is a PostgreSQL feature; skip on other providers (e.g. SQLite in tests).
        if (db.Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) != true)
            return;

        var firstOfMonth = new DateTimeOffset(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, TimeSpan.Zero);

        foreach (var table in PartitionedTables)
        {
            // Provision this month and the next so rows always have a target partition ahead of time.
            for (var offset = 0; offset <= 1; offset++)
            {
                var from = firstOfMonth.AddMonths(offset);
                var to = from.AddMonths(1);
                var partition = $"{table}_y{from.Year}m{from.Month:D2}";

                var sql = $"""
                    CREATE TABLE IF NOT EXISTS "{partition}" PARTITION OF "{table}"
                    FOR VALUES FROM ('{from.ToString("yyyy-MM-dd HH:mm:sszzz", CultureInfo.InvariantCulture)}')
                                 TO ('{to.ToString("yyyy-MM-dd HH:mm:sszzz", CultureInfo.InvariantCulture)}');
                    """;

                await db.Database.ExecuteSqlRawAsync(sql, cancellationToken);
            }
        }

        logger.LogDebug("Ensured monthly partitions through {Month:yyyy-MM}.", firstOfMonth.AddMonths(1));
    }
}
