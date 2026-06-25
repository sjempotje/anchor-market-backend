namespace AnchorMarket.Domain.Enums;

/// <summary>Represents the outcome of a single external feed fetch.</summary>
public enum FeedResultStatus
{
    /// <summary>The fetch succeeded and a value was parsed.</summary>
    Success,
    /// <summary>The fetch reached the source but the request failed (non-success status or transport error).</summary>
    Failed,
    /// <summary>The fetch did not complete within the configured timeout.</summary>
    Timeout,
    /// <summary>The response was received but could not be parsed into a usable value.</summary>
    ParseError
}
