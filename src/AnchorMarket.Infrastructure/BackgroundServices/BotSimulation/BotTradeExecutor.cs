using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Positions.Commands;
using AnchorMarket.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AnchorMarket.Infrastructure.BackgroundServices.BotSimulation;

/// <summary>
/// Places a single realistic bot bet on a market by routing through the same <see cref="PlacePositionCommand"/>
/// that real users use, so bot activity produces identical price movement, price-history points, and realtime
/// events. Outcome selection uses a stable per-market bias (so each market trends toward a "true" price rather
/// than sitting at 50/50) blended with noise, and bet sizes follow a small-heavy distribution.
/// </summary>
public sealed class BotTradeExecutor(
    IServiceScopeFactory scopeFactory,
    IOptions<BotSimulationOptions> options)
{
    private readonly BotSimulationOptions _options = options.Value;

    /// <summary>Places one bet from a random bot on the given market. Returns false if it could not be placed.</summary>
    public async Task<bool> PlaceTradeAsync(BotWorld world, Guid marketId, Random rng, CancellationToken cancellationToken)
    {
        if (world.BotIds.Count == 0)
            return false;

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var outcomeIds = await db.Outcomes
            .Where(o => o.MarketId == marketId)
            .OrderBy(o => o.SortOrder)
            .Select(o => o.Id)
            .ToListAsync(cancellationToken);

        if (outcomeIds.Count == 0)
            return false;

        var botId = world.BotIds[rng.Next(world.BotIds.Count)];
        var amount = RandomAmount(rng);

        var outcomeIndex = PickOutcome(marketId, outcomeIds.Count, rng);
        var outcomeId = outcomeIds[outcomeIndex];

        try
        {
            await EnsureFundsAsync(db, botId, amount, cancellationToken);
            await sender.Send(new PlacePositionCommand(botId, marketId, outcomeId, amount), cancellationToken);
            return true;
        }
        catch (InvalidOperationException)
        {
            // Market closed/resolved or insufficient balance between selection and placement, skip.
            return false;
        }
        catch (DbUpdateException)
        {
            // Two bot trades hit the same shared wallet concurrently (optimistic-concurrency conflict on the
            // wallet version token). Best-effort traffic: just drop this bet rather than retry.
            return false;
        }
    }

    /// <summary>Tops the bot's wallet back up to the starting balance when it can't cover the next bet.</summary>
    private async Task EnsureFundsAsync(IApplicationDbContext db, Guid botId, decimal amount, CancellationToken cancellationToken)
    {
        var wallet = await db.Wallets.FirstOrDefaultAsync(w => w.UserId == botId, cancellationToken);
        if (wallet is null)
        {
            wallet = Wallet.Create(botId);
            wallet.Credit(_options.BotStartingBalance);
            db.Wallets.Add(wallet);
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        if (wallet.Balance < amount)
        {
            wallet.Credit(_options.BotStartingBalance);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static decimal RandomAmount(Random rng)
    {
        var r = rng.NextDouble();
        var whole = r < 0.70 ? rng.Next(5, 41)
                  : r < 0.93 ? rng.Next(40, 151)
                  : rng.Next(150, 801);
        return whole + Math.Round((decimal)rng.NextDouble(), 2);
    }

    /// <summary>
    /// Picks an outcome index. 70% of the time it follows a stable per-market bias so prices trend and settle
    /// realistically; the rest of the time it's uniform, creating the noise/counter-trades that move the line.
    /// </summary>
    private static int PickOutcome(Guid marketId, int outcomeCount, Random rng)
    {
        if (outcomeCount == 1)
            return 0;

        if (rng.NextDouble() >= 0.70)
            return rng.Next(outcomeCount);

        var weights = StableWeights(marketId, outcomeCount);
        var roll = rng.NextDouble() * weights.Sum();
        var cumulative = 0.0;
        for (var i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (roll <= cumulative)
                return i;
        }

        return outcomeCount - 1;
    }

    /// <summary>Deterministic, market-specific outcome weights so each market has its own persistent "shape".</summary>
    private static double[] StableWeights(Guid marketId, int outcomeCount)
    {
        var seed = BitConverter.ToInt32(marketId.ToByteArray(), 0);
        var marketRng = new Random(seed);
        var weights = new double[outcomeCount];
        for (var i = 0; i < outcomeCount; i++)
        {
            // Skew exponent produces some clear favorites and some near-even markets.
            weights[i] = Math.Pow(marketRng.NextDouble() + 0.15, 2);
        }
        return weights;
    }
}
