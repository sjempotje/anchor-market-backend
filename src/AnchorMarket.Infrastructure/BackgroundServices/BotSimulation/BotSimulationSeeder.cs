using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AnchorMarket.Infrastructure.BackgroundServices.BotSimulation;

/// <summary>The seeded, cached state the bot services operate against.</summary>
public sealed record BotWorld(
    IReadOnlyList<Guid> BotIds,
    IReadOnlyDictionary<string, Guid> CategoryIdsBySlug);

/// <summary>
/// Idempotently seeds the bot users (with funded wallets) and discovery categories that the market-factory
/// and trading services depend on. Safe to call from multiple hosted services concurrently: the work runs
/// at most once and the resulting <see cref="BotWorld"/> is cached for the process lifetime.
/// </summary>
public sealed class BotSimulationSeeder(
    IServiceScopeFactory scopeFactory,
    IOptions<BotSimulationOptions> options,
    ILogger<BotSimulationSeeder> logger)
{
    private readonly BotSimulationOptions _options = options.Value;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private BotWorld? _world;

    /// <summary>Ensures categories and bot users exist, returning the cached world after the first run.</summary>
    public async Task<BotWorld> EnsureSeededAsync(CancellationToken cancellationToken)
    {
        if (_world is not null)
            return _world;

        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_world is not null)
                return _world;

            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            var categoryIds = await EnsureCategoriesAsync(db, cancellationToken);
            var botIds = await EnsureBotsAsync(db, cancellationToken);

            _world = new BotWorld(botIds, categoryIds);
            logger.LogInformation("Bot simulation seeded: {BotCount} bots, {CategoryCount} categories.",
                botIds.Count, categoryIds.Count);
            return _world;
        }
        finally
        {
            _gate.Release();
        }
    }

    private static async Task<IReadOnlyDictionary<string, Guid>> EnsureCategoriesAsync(
        IApplicationDbContext db, CancellationToken cancellationToken)
    {
        var existing = await db.Categories
            .ToDictionaryAsync(c => c.Slug, c => c.Id, cancellationToken);

        foreach (var cat in BotContent.Categories)
        {
            if (existing.ContainsKey(cat.Slug))
                continue;

            var entity = Category.Create(cat.Name, cat.Slug, cat.Icon);
            db.Categories.Add(entity);
            existing[cat.Slug] = entity.Id;
        }

        await db.SaveChangesAsync(cancellationToken);
        return existing;
    }

    private async Task<IReadOnlyList<Guid>> EnsureBotsAsync(IApplicationDbContext db, CancellationToken cancellationToken)
    {
        var suffix = "@" + BotContent.BotEmailDomain;
        var existing = await db.Users
            .Where(u => u.Email.EndsWith(suffix))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var botIds = new List<Guid>(existing);
        if (botIds.Count >= _options.BotCount)
            return botIds.Take(_options.BotCount).ToList();

        var takenUsernames = await db.Users
            .Where(u => u.Email.EndsWith(suffix))
            .Select(u => u.Username)
            .ToListAsync(cancellationToken);
        var taken = takenUsernames.Where(n => n is not null).Select(n => n!).ToHashSet();

        foreach (var identity in BotContent.Identities)
        {
            if (botIds.Count >= _options.BotCount)
                break;
            if (taken.Contains(identity.Username))
                continue;

            var user = User.Create(identity.Username, $"{identity.Username}{suffix}");
            var wallet = user.CreateWallet();
            wallet.Credit(_options.BotStartingBalance);

            db.Users.Add(user);
            botIds.Add(user.Id);
        }

        await db.SaveChangesAsync(cancellationToken);
        return botIds;
    }
}
