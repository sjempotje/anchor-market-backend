namespace AnchorMarket.Domain.Enums;

/// <summary>Represents the lifecycle status of an event.</summary>
public enum EventStatus
{
    /// <summary>The event is scheduled but has not started.</summary>
    Upcoming,
    /// <summary>The event is currently in progress.</summary>
    Live,
    /// <summary>The event has concluded.</summary>
    Completed,
    /// <summary>The event was cancelled.</summary>
    Cancelled
}
