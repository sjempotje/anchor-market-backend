namespace AnchorMarket.Domain.Entities;

/// <summary>A streaming source (e.g. YouTube, Twitch) for a live match broadcast.</summary>
public class MatchStream : BaseEntity
{
    /// <summary>Gets the ID of the associated match.</summary>
    public Guid MatchId { get; private set; }

    /// <summary>Gets the streaming provider name (e.g. "YouTube", "Twitch").</summary>
    public string Provider { get; private set; } = string.Empty;

    /// <summary>Gets the URL to access the stream.</summary>
    public string Url { get; private set; } = string.Empty;

    /// <summary>Gets whether a user must be logged in to view the stream.</summary>
    public bool RequiresLogin { get; private set; }

    /// <summary>Gets the associated match.</summary>
    public Match Match { get; private set; } = null!;

    /// <summary>Creates a new match stream.</summary>
    /// <param name="matchId">The ID of the associated match.</param>
    /// <param name="provider">The streaming provider name.</param>
    /// <param name="url">The stream URL.</param>
    /// <param name="requiresLogin">Whether login is required to view.</param>
    /// <returns>A new <see cref="MatchStream"/> instance.</returns>
    public static MatchStream Create(Guid matchId, string provider, string url, bool requiresLogin = false)
    {
        return new MatchStream { MatchId = matchId, Provider = provider, Url = url, RequiresLogin = requiresLogin };
    }
}
