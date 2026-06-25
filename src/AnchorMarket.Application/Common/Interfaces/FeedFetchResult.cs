using AnchorMarket.Domain.Enums;

namespace AnchorMarket.Application.Common.Interfaces;

/// <summary>The pure result of a single feed fetch, decoupled from persistence.</summary>
/// <param name="RawJson">The raw response body, retained verbatim.</param>
/// <param name="ParsedValue">The numeric value extracted from the response, if any.</param>
/// <param name="Status">The outcome of the fetch.</param>
/// <param name="ErrorMessage">Error detail when the fetch did not succeed.</param>
public record FeedFetchResult(
    string RawJson,
    decimal? ParsedValue,
    FeedResultStatus Status,
    string? ErrorMessage = null)
{
    /// <summary>Creates a successful result carrying a parsed value.</summary>
    public static FeedFetchResult Ok(string rawJson, decimal parsedValue)
        => new(rawJson, parsedValue, FeedResultStatus.Success);

    /// <summary>Creates a successful result that carries no numeric value (raw passthrough).</summary>
    public static FeedFetchResult Raw(string rawJson)
        => new(rawJson, null, FeedResultStatus.Success);

    /// <summary>Creates a failed result.</summary>
    public static FeedFetchResult Failure(FeedResultStatus status, string errorMessage, string rawJson = "")
        => new(rawJson, null, status, errorMessage);
}
