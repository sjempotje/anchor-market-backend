namespace AnchorMarket.Infrastructure.BackgroundServices.BotSimulation;

/// <summary>
/// Configuration for the bot-simulation subsystem, bound from the "BotSimulation" configuration section.
/// When enabled, seeded bot users automatically create public markets and trade on them to simulate
/// a lively, high-traffic platform. Intended for development and demo environments only.
/// </summary>
public class BotSimulationOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "BotSimulation";

    /// <summary>Master switch. When false, no bots, markets, or trades are generated.</summary>
    public bool Enabled { get; set; }

    /// <summary>Number of bot users to seed. These act as both market creators and traders.</summary>
    public int BotCount { get; set; } = 30;

    /// <summary>Virtual-currency balance each bot wallet is seeded with (and topped back up to when low).</summary>
    public decimal BotStartingBalance { get; set; } = 1_000_000m;

    /// <summary>How many public markets to ensure exist on startup (bulk backfill).</summary>
    public int InitialMarketCount { get; set; } = 60;

    /// <summary>Upper bound on the number of open public bot markets kept alive at once.</summary>
    public int MaxOpenMarkets { get; set; } = 120;

    /// <summary>Minimum delay between automatic new-market creations.</summary>
    public TimeSpan MarketCreationMinInterval { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>Maximum delay between automatic new-market creations.</summary>
    public TimeSpan MarketCreationMaxInterval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>How often the trading loop wakes to place bets. Shorter ticks spread activity more evenly.</summary>
    public TimeSpan TradeTickInterval { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Target trades per open market per minute. The trading loop scales the number of bets it places each tick
    /// with the current market count so every market stays roughly this active regardless of how many exist.
    /// </summary>
    public double TargetTradesPerMarketPerMinute { get; set; } = 5.0;

    /// <summary>Safety cap on how many bets a single trading tick may place, to bound database load.</summary>
    public int MaxTradesPerTick { get; set; } = 250;

    /// <summary>Number of seed bets placed on a market right after it is created, to give it initial liquidity.</summary>
    public int InitialTradesPerMarket { get; set; } = 8;
}
