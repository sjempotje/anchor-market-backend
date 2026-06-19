namespace AnchorMarket.Domain.Enums;

/// <summary>Represents the current stage of a match.</summary>
public enum MatchStatus
{
    /// <summary>The match is scheduled but has not started.</summary>
    Scheduled,
    /// <summary>The match is currently being played.</summary>
    Live,
    /// <summary>The match has ended.</summary>
    Finished,
    /// <summary>The match was postponed.</summary>
    Postponed,
    /// <summary>The match was cancelled.</summary>
    Cancelled
}
