namespace AnchorMarket.Domain.Enums;

/// <summary>Determines the visibility scope of a market.</summary>
public enum MarketScope
{
    /// <summary>The market is visible to all users.</summary>
    Public,
    /// <summary>The market is restricted to members of a specific group.</summary>
    Group
}
