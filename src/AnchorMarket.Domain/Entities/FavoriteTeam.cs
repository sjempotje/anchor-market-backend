namespace AnchorMarket.Domain.Entities;

/// <summary>Tracks a user's favorited sports team.</summary>
public class FavoriteTeam : BaseEntity
{
    /// <summary>Gets the ID of the user who favorited the team.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Gets the ID of the favorited team.</summary>
    public Guid TeamId { get; private set; }

    /// <summary>Gets the favorited team.</summary>
    public Team Team { get; private set; } = null!;

    /// <summary>Creates a new favorite team record for a user.</summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="teamId">The team's ID.</param>
    /// <returns>A new <see cref="FavoriteTeam"/> instance.</returns>
    public static FavoriteTeam Create(Guid userId, Guid teamId)
    {
        return new FavoriteTeam { UserId = userId, TeamId = teamId };
    }
}
