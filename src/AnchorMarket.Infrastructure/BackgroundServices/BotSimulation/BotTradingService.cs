using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AnchorMarket.Infrastructure.BackgroundServices.BotSimulation;

/// <summary>
/// Drives continuous, realistic-looking traffic. On a fixed short interval it places bot bets spread across
/// randomly chosen open public markets. The number of bets per tick scales with the current market count to hold
/// a target per-market trade rate (see <see cref="BotSimulationOptions.TargetTradesPerMarketPerMinute"/>), so every
/// market stays active regardless of how many exist. The candidate list is refreshed periodically, not per tick.
/// </summary>
public sealed class BotTradingService(
    IServiceScopeFactory scopeFactory,
    BotSimulationSeeder seeder,
    BotTradeExecutor tradeExecutor,
    IOptions<BotSimulationOptions> options,
    ILogger<BotTradingService> logger) : BackgroundService
{
    private static readonly TimeSpan CandidateRefreshInterval = TimeSpan.FromSeconds(20);

    private readonly BotSimulationOptions _options = options.Value;
    private readonly Random _rng = new();

    private IReadOnlyList<Guid> _candidateMarketIds = [];
    private DateTimeOffset _candidatesRefreshedAt = DateTimeOffset.MinValue;

    // Carries the fractional part of the desired trade count between ticks so the target rate is hit exactly
    // even when a tick's quota is less than one trade (e.g. few markets or a very short tick interval).
    private double _tradeCarry;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("BotTradingService started (target {Rate} trades/market/min).",
            _options.TargetTradesPerMarketPerMinute);

        BotWorld world;
        try
        {
            world = await seeder.EnsureSeededAsync(stoppingToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Bot trading startup failed.");
            return;
        }

        using var timer = new PeriodicTimer(_options.TradeTickInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await RunTickAsync(world, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Bot trading tick failed.");
            }
        }
    }

    private async Task RunTickAsync(BotWorld world, CancellationToken cancellationToken)
    {
        var candidates = await GetCandidateMarketsAsync(world, cancellationToken);
        if (candidates.Count == 0)
            return;

        // Trades this tick = markets × per-market-per-minute rate × (tick seconds / 60), plus any carried remainder.
        var tickMinutes = _options.TradeTickInterval.TotalMinutes;
        _tradeCarry += candidates.Count * _options.TargetTradesPerMarketPerMinute * tickMinutes;

        var trades = (int)_tradeCarry;
        _tradeCarry -= trades;

        // Bound per-tick work; drop any excess carry so it can't spiral if ticks fall behind.
        if (trades > _options.MaxTradesPerTick)
        {
            trades = _options.MaxTradesPerTick;
            _tradeCarry = 0;
        }

        for (var i = 0; i < trades && !cancellationToken.IsCancellationRequested; i++)
        {
            var marketId = candidates[_rng.Next(candidates.Count)];
            await tradeExecutor.PlaceTradeAsync(world, marketId, _rng, cancellationToken);
        }
    }

    private async Task<IReadOnlyList<Guid>> GetCandidateMarketsAsync(BotWorld world, CancellationToken cancellationToken)
    {
        if (DateTimeOffset.UtcNow - _candidatesRefreshedAt < CandidateRefreshInterval && _candidateMarketIds.Count > 0)
            return _candidateMarketIds;

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        _candidateMarketIds = await db.Markets
            .Where(m => m.Scope == MarketScope.Public
                        && m.Status == MarketStatus.Open
                        && world.BotIds.Contains(m.CreatorId))
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);
        _candidatesRefreshedAt = DateTimeOffset.UtcNow;

        return _candidateMarketIds;
    }
}
