using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Markets.Commands;
using AnchorMarket.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AnchorMarket.Infrastructure.BackgroundServices.BotSimulation;

/// <summary>
/// Keeps the platform stocked with public markets. On startup it bulk-backfills up to a configured target so the
/// database looks populated immediately; thereafter it creates one fresh, randomized market every 1–5 minutes
/// (up to a cap). Each new market is seeded with a handful of bot bets so it opens with liquidity and a non-flat price.
/// </summary>
public sealed class MarketFactoryService(
    IServiceScopeFactory scopeFactory,
    BotSimulationSeeder seeder,
    BotTradeExecutor tradeExecutor,
    IOptions<BotSimulationOptions> options,
    ILogger<MarketFactoryService> logger) : BackgroundService
{
    private readonly BotSimulationOptions _options = options.Value;
    private readonly Random _rng = new();

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("MarketFactoryService started.");

        BotWorld world;
        try
        {
            world = await seeder.EnsureSeededAsync(stoppingToken);
            await BackfillAsync(world, stoppingToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Market factory startup/backfill failed.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = RandomInterval(_options.MarketCreationMinInterval, _options.MarketCreationMaxInterval);
            try
            {
                await Task.Delay(delay, stoppingToken);

                if (await CountOpenMarketsAsync(world, stoppingToken) >= _options.MaxOpenMarkets)
                    continue;

                await CreateMarketAsync(world, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Automatic market creation failed.");
            }
        }
    }

    private async Task BackfillAsync(BotWorld world, CancellationToken cancellationToken)
    {
        var existing = await CountOpenMarketsAsync(world, cancellationToken);
        var toCreate = _options.InitialMarketCount - existing;
        if (toCreate <= 0)
            return;

        logger.LogInformation("Backfilling {Count} public markets.", toCreate);
        for (var i = 0; i < toCreate && !cancellationToken.IsCancellationRequested; i++)
        {
            try
            {
                await CreateMarketAsync(world, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Backfill market creation failed.");
            }
        }
    }

    private async Task<int> CountOpenMarketsAsync(BotWorld world, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        return await db.Markets.CountAsync(
            m => m.Scope == MarketScope.Public
                 && m.Status == MarketStatus.Open
                 && world.BotIds.Contains(m.CreatorId),
            cancellationToken);
    }

    private async Task CreateMarketAsync(BotWorld world, CancellationToken cancellationToken)
    {
        var spec = BotContent.Generate(_rng);
        var creatorId = world.BotIds[_rng.Next(world.BotIds.Count)];
        var categoryId = world.CategoryIdsBySlug.GetValueOrDefault(spec.CategorySlug);
        var slug = BuildSlug(spec.Title);
        var imageUrl = $"https://picsum.photos/seed/{slug}/600/400";
        var deadline = DateTimeOffset.UtcNow.AddDays(spec.ResolutionDays);

        Guid marketId;
        using (var scope = scopeFactory.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            marketId = await sender.Send(new CreateMarketCommand(
                spec.Title,
                spec.Description,
                deadline,
                MarketScope.Public,
                creatorId,
                GroupId: null,
                spec.Outcomes,
                spec.MarketType,
                categoryId == Guid.Empty ? null : categoryId,
                imageUrl,
                slug), cancellationToken);
        }

        // Seed initial liquidity so the market opens with a believable price and volume.
        var seedTrades = _rng.Next(_options.InitialTradesPerMarket / 2, _options.InitialTradesPerMarket + 1);
        for (var i = 0; i < seedTrades && !cancellationToken.IsCancellationRequested; i++)
        {
            await tradeExecutor.PlaceTradeAsync(world, marketId, _rng, cancellationToken);
        }

        logger.LogDebug("Created market '{Title}' ({MarketId}) with {Trades} seed trades.", spec.Title, marketId, seedTrades);
    }

    private TimeSpan RandomInterval(TimeSpan min, TimeSpan max)
    {
        if (max <= min)
            return min;
        var span = (max - min).TotalMilliseconds;
        return min + TimeSpan.FromMilliseconds(_rng.NextDouble() * span);
    }

    private static string BuildSlug(string title)
    {
        var chars = title.ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray();
        var slug = new string(chars);
        while (slug.Contains("--"))
            slug = slug.Replace("--", "-");
        slug = slug.Trim('-');
        if (slug.Length > 60)
            slug = slug[..60].Trim('-');
        // Short suffix keeps slugs unique across repeated/similar titles.
        return $"{slug}-{Guid.NewGuid():N}"[..Math.Min(72, slug.Length + 7)];
    }
}
