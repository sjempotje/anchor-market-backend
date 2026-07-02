namespace AnchorMarket.Domain.Enums;

/// <summary>Defines the category of a notification sent to a user.</summary>
public enum NotificationType
{
    /// <summary>Notification triggered by a price movement on a watched market.</summary>
    PriceAlert,
    /// <summary>Notification that a market has been resolved.</summary>
    MarketResolved,
    /// <summary>Notification that a new market has been created.</summary>
    MarketCreated
}
