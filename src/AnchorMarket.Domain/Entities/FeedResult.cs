using AnchorMarket.Domain.Enums;

namespace AnchorMarket.Domain.Entities;

/// <summary>A raw response captured from an external feed, retained for auditing and replay.</summary>
public class FeedResult : BaseEntity
{
    /// <summary>Gets the ID of the feed registration that produced this result.</summary>
    public Guid FeedRegistrationId { get; private set; }

    /// <summary>Gets the raw response body exactly as received from the source.</summary>
    public string RawJson { get; private set; } = string.Empty;

    /// <summary>Gets the numeric value parsed from the response, or null when none could be extracted.</summary>
    public decimal? ParsedValue { get; private set; }

    /// <summary>Gets the outcome of the fetch.</summary>
    public FeedResultStatus Status { get; private set; }

    /// <summary>Gets the error detail when the fetch did not succeed.</summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>Gets the timestamp when the response was received.</summary>
    public DateTimeOffset ReceivedAt { get; private set; }

    /// <summary>Gets the feed registration that produced this result.</summary>
    public ExternalFeedRegistration Registration { get; private set; } = null!;

    /// <summary>Creates a new feed result.</summary>
    /// <param name="feedRegistrationId">The feed registration that produced this result.</param>
    /// <param name="rawJson">The raw response body.</param>
    /// <param name="parsedValue">The parsed numeric value, if any.</param>
    /// <param name="status">The outcome of the fetch.</param>
    /// <param name="errorMessage">Error detail when the fetch failed.</param>
    /// <param name="receivedAt">When the response was received.</param>
    /// <returns>A new <see cref="FeedResult"/> instance.</returns>
    public static FeedResult Create(
        Guid feedRegistrationId,
        string rawJson,
        decimal? parsedValue,
        FeedResultStatus status,
        string? errorMessage,
        DateTimeOffset receivedAt)
    {
        return new FeedResult
        {
            FeedRegistrationId = feedRegistrationId,
            RawJson = rawJson,
            ParsedValue = parsedValue,
            Status = status,
            ErrorMessage = errorMessage,
            ReceivedAt = receivedAt
        };
    }
}
