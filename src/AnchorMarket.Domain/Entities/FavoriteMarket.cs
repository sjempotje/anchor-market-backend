namespace AnchorMarket.Domain.Entities;

/// <summary>Tracks a user's favorited prediction market.</summary>
public class FavoriteMarket : BaseEntity
{
    /// <summary>Gets the ID of the user who favorited the market.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Gets the ID of the favorited market.</summary>
    public Guid MarketId { get; private set; }

    /// <summary>Gets the favorited market.</summary>
    public Market Market { get; private set; } = null!;

    /// <summary>Creates a new favorite market record for a user.</summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="marketId">The market's ID.</param>
    /// <returns>A new <see cref="FavoriteMarket"/> instance.</returns>
    public static FavoriteMarket Create(Guid userId, Guid marketId)
    {
        return new FavoriteMarket { UserId = userId, MarketId = marketId };
    }
}
