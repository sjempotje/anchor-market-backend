using AnchorMarket.Domain.Enums;

namespace AnchorMarket.Domain.Entities;

/// <summary>A sports match between two teams, used to anchor sports-based prediction markets.</summary>
public class Match : BaseEntity
{
    /// <summary>Gets the ID of the home team.</summary>
    public Guid HomeTeamId { get; private set; }

    /// <summary>Gets the ID of the away team.</summary>
    public Guid AwayTeamId { get; private set; }

    /// <summary>Gets the scheduled kick-off or start time of the match.</summary>
    public DateTimeOffset StartTime { get; private set; }

    /// <summary>Gets the current status of the match.</summary>
    public MatchStatus Status { get; private set; } = MatchStatus.Scheduled;

    /// <summary>Gets the ID of the league this match belongs to.</summary>
    public Guid LeagueId { get; private set; }

    /// <summary>Populated when the match belongs to a tournament/event.</summary>
    public Guid? EventId { get; private set; }

    /// <summary>Gets the home team.</summary>
    public Team HomeTeam { get; private set; } = null!;

    /// <summary>Gets the away team.</summary>
    public Team AwayTeam { get; private set; } = null!;

    /// <summary>Gets the league this match is played in.</summary>
    public League League { get; private set; } = null!;

    /// <summary>Gets the tournament or event this match is part of, if any.</summary>
    public Event? Event { get; private set; }

    /// <summary>Gets the live state (score, clock, period) of the match.</summary>
    public MatchState? State { get; private set; }

    /// <summary>Gets the available live-stream links for the match.</summary>
    public ICollection<MatchStream> Streams { get; private set; } = new List<MatchStream>();

    /// <summary>Gets the prediction markets based on this match.</summary>
    public ICollection<Market> Markets { get; private set; } = new List<Market>();

    /// <summary>Creates a new match.</summary>
    /// <param name="homeTeamId">ID of the home team.</param>
    /// <param name="awayTeamId">ID of the away team.</param>
    /// <param name="leagueId">ID of the league.</param>
    /// <param name="startTime">Scheduled start time.</param>
    /// <param name="eventId">Optional tournament event ID.</param>
    /// <returns>A new <see cref="Match"/> instance.</returns>
    public static Match Create(Guid homeTeamId, Guid awayTeamId, Guid leagueId,
        DateTimeOffset startTime, Guid? eventId = null)
    {
        return new Match
        {
            HomeTeamId = homeTeamId,
            AwayTeamId = awayTeamId,
            LeagueId = leagueId,
            StartTime = startTime,
            EventId = eventId
        };
    }

    public void UpdateStatus(MatchStatus status)
    {
        Status = status;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
