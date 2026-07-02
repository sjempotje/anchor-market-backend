using System.Globalization;
using AnchorMarket.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AnchorMarket.Infrastructure.BackgroundServices;

/// <summary>
/// Ensures the current and next month range partitions exist for the partitioned time-series tables,
/// so incoming rows land in a monthly partition rather than the catch-all default. Runs at startup
/// and daily. PostgreSQL only. Failures are isolated per partition and never stop the host.
/// </summary>
public class PartitionManagerService(
    IServiceScopeFactory scopeFactory,
    ILogger<PartitionManagerService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    /// <summary>Partitioned tables paired with their range (timestamp) column.</summary>
    private static readonly (string Table, string Column)[] PartitionedTables =
    [
    ];

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("PartitionManagerService started.");

        // Guarded so a provisioning failure at startup never brings down the host.
        try
        {
            await EnsurePartitionsAsync(stoppingToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Initial partition provisioning failed.");
        }

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

        foreach (var (table, column) in PartitionedTables)
        {
            // Provision this month and the next so rows always have a target partition ahead of time.
            for (var offset = 0; offset <= 1; offset++)
            {
                var from = firstOfMonth.AddMonths(offset);
                var to = from.AddMonths(1);
                try
                {
                    await EnsurePartitionAsync(db, table, column, from, to, cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Failed to provision partition for {Table} {Month:yyyy-MM}.", table, from);
                }
            }
        }
    }

    private async Task EnsurePartitionAsync(
        IApplicationDbContext db, string table, string column, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        var partition = $"{table}_y{from.Year}m{from.Month:D2}";
        var fromValue = Format(from);
        var toValue = Format(to);

        try
        {
            await db.Database.ExecuteSqlRawAsync(
                $"""CREATE TABLE IF NOT EXISTS "{partition}" PARTITION OF "{table}" FOR VALUES FROM ('{fromValue}') TO ('{toValue}');""",
                cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.CheckViolation)
        {
            // The default partition already holds rows in this range. Migrate them into a fresh
            // monthly partition: detach default, create the partition, move the matching rows, reattach.
            var defaultPartition = $"{table}_default";
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

            await db.Database.ExecuteSqlRawAsync($"""ALTER TABLE "{table}" DETACH PARTITION "{defaultPartition}";""", cancellationToken);
            await db.Database.ExecuteSqlRawAsync(
                $"""CREATE TABLE "{partition}" PARTITION OF "{table}" FOR VALUES FROM ('{fromValue}') TO ('{toValue}');""", cancellationToken);
            await db.Database.ExecuteSqlRawAsync(
                $"""INSERT INTO "{table}" SELECT * FROM "{defaultPartition}" WHERE "{column}" >= '{fromValue}' AND "{column}" < '{toValue}';""", cancellationToken);
            await db.Database.ExecuteSqlRawAsync(
                $"""DELETE FROM "{defaultPartition}" WHERE "{column}" >= '{fromValue}' AND "{column}" < '{toValue}';""", cancellationToken);
            await db.Database.ExecuteSqlRawAsync($"""ALTER TABLE "{table}" ATTACH PARTITION "{defaultPartition}" DEFAULT;""", cancellationToken);

            await tx.CommitAsync(cancellationToken);
            logger.LogInformation("Migrated default rows into new partition {Partition}.", partition);
        }
    }

    private static string Format(DateTimeOffset value) => value.ToString("yyyy-MM-dd HH:mm:sszzz", CultureInfo.InvariantCulture);
}
