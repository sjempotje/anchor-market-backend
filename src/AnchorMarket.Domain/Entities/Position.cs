namespace AnchorMarket.Domain.Entities;

/// <summary>A user's stake on a specific market outcome.</summary>
public class Position : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid OutcomeId { get; private set; }

    /// <summary>Amount of virtual currency staked.</summary>
    public decimal Amount { get; private set; }

    /// <summary>Shares purchased at the time of the bet.</summary>
    public decimal Shares { get; private set; }

    public Outcome Outcome { get; private set; } = null!;
}
