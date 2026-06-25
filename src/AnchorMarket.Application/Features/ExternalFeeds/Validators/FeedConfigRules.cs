using System.Text.Json;

namespace AnchorMarket.Application.Features.ExternalFeeds.Validators;

/// <summary>Shared validation helpers for feed configuration payloads.</summary>
internal static class FeedConfigRules
{
    /// <summary>Returns whether the given string is null/empty or a well-formed JSON document.</summary>
    public static bool BeValidJson(string? config)
    {
        if (string.IsNullOrWhiteSpace(config))
            return true;

        try
        {
            using var _ = JsonDocument.Parse(config);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
