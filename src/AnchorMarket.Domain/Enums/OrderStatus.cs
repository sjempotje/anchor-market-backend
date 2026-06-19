namespace AnchorMarket.Domain.Enums;

/// <summary>Represents the lifecycle status of an order.</summary>
public enum OrderStatus
{
    /// <summary>The order has been submitted and is waiting to be matched.</summary>
    Pending,
    /// <summary>The order has been partially filled.</summary>
    PartiallyFilled,
    /// <summary>The order has been completely filled.</summary>
    Filled,
    /// <summary>The order was cancelled by the user.</summary>
    Canceled,
    /// <summary>The order expired without being filled.</summary>
    Expired,
    /// <summary>The order was rejected by the system.</summary>
    Rejected
}
