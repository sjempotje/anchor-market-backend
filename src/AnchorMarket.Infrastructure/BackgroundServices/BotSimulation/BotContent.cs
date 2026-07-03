using AnchorMarket.Domain.Enums;

namespace AnchorMarket.Infrastructure.BackgroundServices.BotSimulation;

/// <summary>A market produced by a generator, ready to be turned into a domain market.</summary>
public sealed record GeneratedMarket(
    string Title,
    string Description,
    string CategorySlug,
    MarketType MarketType,
    IReadOnlyList<string> Outcomes,
    int ResolutionDays);

/// <summary>A category to ensure exists for bot-generated markets.</summary>
public sealed record BotCategory(string Name, string Slug, string Icon);

/// <summary>A bot user identity.</summary>
public sealed record BotIdentity(string Username);

/// <summary>
/// Static content pools and market generators used to produce realistic-looking public markets and bot users.
/// Generators fill templates from randomized pools so the stream of created markets stays varied and non-duplicative.
/// </summary>
public static class BotContent
{
    /// <summary>Email domain used to mark and re-identify seeded bot accounts (never collides with real users).</summary>
    public const string BotEmailDomain = "anchorbots.local";

    /// <summary>Categories bot markets are filed under.</summary>
    public static readonly IReadOnlyList<BotCategory> Categories =
    [
        new("Politics", "politics", "landmark"),
        new("Sports", "sports", "trophy"),
        new("Crypto", "crypto", "bitcoin"),
        new("Technology", "technology", "cpu"),
        new("Entertainment", "entertainment", "clapperboard"),
        new("Economics", "economics", "chart-line"),
        new("Science", "science", "flask"),
        new("World", "world", "globe"),
    ];

    /// <summary>Bot usernames.</summary>
    public static readonly IReadOnlyList<BotIdentity> Identities =
    [
        new("market_maven"), new("alpha_hunter"), new("hedge_hollis"), new("prob_pilot"),
        new("degen_dana"), new("value_vera"), new("longshot_lee"), new("quant_quinn"),
        new("oracle_omar"), new("sharp_sasha"), new("bull_bennett"), new("bear_bianca"),
        new("tail_tucker"), new("edge_esme"), new("parlay_pat"), new("liquidity_liam"),
        new("momentum_mira"), new("contrarian_cole"), new("delta_dev"), new("kelly_king"),
        new("vega_vince"), new("spread_sky"), new("arb_arden"), new("payout_penny"),
        new("ticker_theo"), new("wager_wren"), new("odds_olivia"), new("fade_felix"),
        new("signal_sage"), new("chalk_chris"), new("upset_ursula"), new("book_blake"),
        new("margin_max"), new("risk_remy"), new("prime_priya"), new("swing_stevie"),
    ];

    private static readonly string[] SportsTeams =
    [
        "Lakers", "Celtics", "Warriors", "Heat", "Nuggets", "Bucks", "Suns", "Knicks",
        "Real Madrid", "Barcelona", "Manchester City", "Liverpool", "Arsenal", "Bayern Munich",
        "Chiefs", "49ers", "Eagles", "Cowboys", "Ravens", "Bills",
    ];

    private static readonly string[] Cryptos = ["Bitcoin", "Ethereum", "Solana", "XRP", "Dogecoin", "Cardano", "Avalanche"];
    private static readonly int[] BtcLevels = [80_000, 90_000, 100_000, 120_000, 150_000, 200_000];
    private static readonly int[] EthLevels = [3_000, 4_000, 5_000, 6_000, 8_000, 10_000];

    private static readonly string[] TechCompanies = ["Apple", "Nvidia", "OpenAI", "Google", "Microsoft", "Tesla", "Meta", "Amazon"];
    private static readonly string[] TechEvents =
    [
        "ship a new flagship AI model", "announce a foldable device", "cross a $4T market cap",
        "release a mixed-reality headset", "launch a robotaxi service", "acquire a major startup",
    ];

    private static readonly string[] Politicians =
    [
        "the incumbent party", "the opposition", "an independent candidate",
        "the centre-left coalition", "the centre-right coalition",
    ];
    private static readonly string[] Countries = ["the US", "the UK", "France", "Germany", "India", "Brazil", "Japan", "Canada"];

    private static readonly string[] Awards = ["Best Picture", "Album of the Year", "Best Actor", "Game of the Year"];
    private static readonly string[] Nominees =
    [
        "the festival favorite", "the box-office hit", "the critics' darling",
        "the indie underdog", "the franchise sequel", "the streaming exclusive",
    ];

    private static readonly string[] EconIndicators =
    [
        "cut interest rates at the next meeting", "hold rates steady this quarter",
        "report inflation below 3%", "report unemployment under 4%",
    ];

    private static readonly string[] ScienceEvents =
    [
        "a crewed mission launch to the Moon this year", "a room-temperature superconductor replication",
        "a new exoplanet confirmed in the habitable zone", "a fusion reactor reaching net energy gain",
    ];

    private static readonly (string Region, string Event)[] WorldEvents =
    [
        ("global", "a new record global average temperature this year"),
        ("energy", "oil closing above $100 a barrel this year"),
        ("space", "a private company landing humans on Mars this decade"),
        ("tech", "a nation banning a major social app this year"),
    ];

    /// <summary>All market generators. Each fills a template from the pools above with the supplied RNG.</summary>
    private static readonly Func<Random, GeneratedMarket>[] Generators =
    [
        GenerateSportsMatchup,
        GenerateCryptoThreshold,
        GenerateTechEvent,
        GeneratePoliticsRace,
        GenerateAward,
        GenerateEconomics,
        GenerateScience,
        GenerateWorld,
    ];

    /// <summary>Produces one randomized, realistic market definition.</summary>
    public static GeneratedMarket Generate(Random rng) => Generators[rng.Next(Generators.Length)](rng);

    private static GeneratedMarket GenerateSportsMatchup(Random rng)
    {
        var a = SportsTeams[rng.Next(SportsTeams.Length)];
        string b;
        do { b = SportsTeams[rng.Next(SportsTeams.Length)]; } while (b == a);
        return new GeneratedMarket(
            $"{a} vs {b}: who wins?",
            $"Resolves to the team that wins the upcoming {a} versus {b} fixture. Postponed or abandoned matches roll to the rescheduled date.",
            "sports", MarketType.Moneyline,
            [a, b], rng.Next(2, 21));
    }

    private static GeneratedMarket GenerateCryptoThreshold(Random rng)
    {
        var coin = Cryptos[rng.Next(Cryptos.Length)];
        var (level, unit) = coin switch
        {
            "Bitcoin" => (BtcLevels[rng.Next(BtcLevels.Length)], "$"),
            "Ethereum" => (EthLevels[rng.Next(EthLevels.Length)], "$"),
            _ => (rng.Next(2, 20) * 50, "$"),
        };
        var days = rng.Next(14, 91);
        return new GeneratedMarket(
            $"Will {coin} trade above {unit}{level:N0} within {days} days?",
            $"Resolves YES if the {coin} spot price is at or above {unit}{level:N0} on any major exchange before the deadline, per the reference index.",
            "crypto", MarketType.Binary,
            ["Yes", "No"], days);
    }

    private static GeneratedMarket GenerateTechEvent(Random rng)
    {
        var company = TechCompanies[rng.Next(TechCompanies.Length)];
        var ev = TechEvents[rng.Next(TechEvents.Length)];
        var days = rng.Next(30, 181);
        return new GeneratedMarket(
            $"Will {company} {ev} before the deadline?",
            $"Resolves YES if {company} officially {ev} on or before the resolution date, confirmed by a first-party announcement.",
            "technology", MarketType.Binary,
            ["Yes", "No"], days);
    }

    private static GeneratedMarket GeneratePoliticsRace(Random rng)
    {
        var country = Countries[rng.Next(Countries.Length)];
        var picks = Politicians.OrderBy(_ => rng.Next()).Take(3).ToList();
        return new GeneratedMarket(
            $"Who wins the next major election in {country}?",
            $"Resolves to the winning side of the next nationwide vote in {country}, per the official certified result.",
            "politics", MarketType.MultiChoice,
            picks, rng.Next(30, 181));
    }

    private static GeneratedMarket GenerateAward(Random rng)
    {
        var award = Awards[rng.Next(Awards.Length)];
        var picks = Nominees.OrderBy(_ => rng.Next()).Take(4).ToList();
        return new GeneratedMarket(
            $"Which nominee takes home {award}?",
            $"Resolves to the recipient of {award} at the upcoming ceremony, per the official announcement on the night.",
            "entertainment", MarketType.Winner,
            picks, rng.Next(20, 121));
    }

    private static GeneratedMarket GenerateEconomics(Random rng)
    {
        var indicator = EconIndicators[rng.Next(EconIndicators.Length)];
        var days = rng.Next(14, 91);
        return new GeneratedMarket(
            $"Will the central bank {indicator}?",
            $"Resolves YES if the central bank is reported to {indicator} before the deadline, per the official release.",
            "economics", MarketType.Binary,
            ["Yes", "No"], days);
    }

    private static GeneratedMarket GenerateScience(Random rng)
    {
        var ev = ScienceEvents[rng.Next(ScienceEvents.Length)];
        var days = rng.Next(60, 271);
        return new GeneratedMarket(
            $"Will we see {ev}?",
            $"Resolves YES on credible confirmation of {ev} before the resolution date.",
            "science", MarketType.Binary,
            ["Yes", "No"], days);
    }

    private static GeneratedMarket GenerateWorld(Random rng)
    {
        var (_, ev) = WorldEvents[rng.Next(WorldEvents.Length)];
        var days = rng.Next(30, 271);
        return new GeneratedMarket(
            $"Will there be {ev}?",
            $"Resolves YES on credible reporting of {ev} before the deadline.",
            "world", MarketType.Binary,
            ["Yes", "No"], days);
    }
}
