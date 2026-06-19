namespace AnchorMarket.Domain.Enums;

/// <summary>Represents the lifecycle status of a market.</summary>
public enum MarketStatus
{
    /// <summary>The market is accepting orders.</summary>
    Open,
    /// <summary>The market is no longer accepting orders but is not yet resolved.</summary>
    Closed,
    /// <summary>The market has been resolved with a final outcome.</summary>
    Resolved,
    /// <summary>The market was cancelled before resolution.</summary>
    Cancelled
}
