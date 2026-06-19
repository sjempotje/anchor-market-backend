namespace AnchorMarket.Domain.Entities;

public class MatchState : BaseEntity
{
    public Guid MatchId { get; private set; }
    public int ScoreHome { get; private set; }
    public int ScoreAway { get; private set; }

    /// <summary>Current period, map, quarter, half, etc.</summary>
    public string? CurrentPeriod { get; private set; }

    /// <summary>Match clock or elapsed time string (e.g. "73:24").</summary>
    public string? Clock { get; private set; }

    /// <summary>Extra structured info, rounds, maps, sets stored as JSON string.</summary>
    public string? ExtraInfo { get; private set; }

    public Match Match { get; private set; } = null!;

    public static MatchState Create(Guid matchId)
    {
        return new MatchState { MatchId = matchId };
    }

    public void Update(int scoreHome, int scoreAway, string? currentPeriod, string? clock, string? extraInfo = null)
    {
        ScoreHome = scoreHome;
        ScoreAway = scoreAway;
        CurrentPeriod = currentPeriod;
        Clock = clock;
        ExtraInfo = extraInfo;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
