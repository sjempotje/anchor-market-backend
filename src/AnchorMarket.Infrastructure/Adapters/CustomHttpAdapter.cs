using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using AnchorMarket.Domain.Enums;

namespace AnchorMarket.Infrastructure.Adapters;

/// <summary>Fetches a value from an arbitrary JSON HTTP endpoint.</summary>
/// <remarks>
/// Reads <see cref="ExternalFeedRegistration.ApiUrl"/> as the endpoint and, optionally,
/// <see cref="ExternalFeedRegistration.AuthToken"/> as a bearer token. Config may specify a dotted
/// <c>"JsonPath"</c> (e.g. <c>{ "JsonPath": "data.price" }</c>) used to extract a numeric value; when
/// omitted the raw response is stored with no parsed value.
/// </remarks>
public class CustomHttpAdapter(IHttpClientFactory httpClientFactory) : IExternalFeedAdapter
{
    /// <inheritdoc />
    public string AdapterType => "CustomHttp";

    /// <inheritdoc />
    public async Task<FeedFetchResult> FetchAsync(ExternalFeedRegistration registration, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(registration.ApiUrl))
            return FeedFetchResult.Failure(FeedResultStatus.Failed, "CustomHttp feed requires an ApiUrl.");

        string? jsonPath;
        try
        {
            using var configDoc = JsonDocument.Parse(string.IsNullOrWhiteSpace(registration.Config) ? "{}" : registration.Config);
            jsonPath = configDoc.RootElement.TryGetProperty("JsonPath", out var pathElement)
                ? pathElement.GetString()
                : null;
        }
        catch (JsonException ex)
        {
            return FeedFetchResult.Failure(FeedResultStatus.ParseError, $"Invalid feed config JSON: {ex.Message}");
        }

        var client = httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromMilliseconds(registration.TimeoutMs <= 0 ? 3000 : registration.TimeoutMs);

        using var request = new HttpRequestMessage(HttpMethod.Get, registration.ApiUrl);
        if (!string.IsNullOrWhiteSpace(registration.AuthToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", registration.AuthToken);

        string body;
        try
        {
            using var response = await client.SendAsync(request, cancellationToken);
            body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
                return FeedFetchResult.Failure(FeedResultStatus.Failed, $"Feed returned HTTP {(int)response.StatusCode}.", body);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return FeedFetchResult.Failure(FeedResultStatus.Timeout, "Feed request timed out.");
        }
        catch (HttpRequestException ex)
        {
            return FeedFetchResult.Failure(FeedResultStatus.Failed, ex.Message);
        }

        if (string.IsNullOrWhiteSpace(jsonPath))
            return FeedFetchResult.Raw(body);

        try
        {
            using var responseDoc = JsonDocument.Parse(body);
            var element = ResolvePath(responseDoc.RootElement, jsonPath);
            if (element is null)
                return FeedFetchResult.Failure(FeedResultStatus.ParseError, $"JsonPath '{jsonPath}' was not found in the response.", body);

            var value = ExtractDecimal(element.Value);
            if (value is null)
                return FeedFetchResult.Failure(FeedResultStatus.ParseError, $"Value at '{jsonPath}' is not numeric.", body);

            return FeedFetchResult.Ok(body, value.Value);
        }
        catch (JsonException ex)
        {
            return FeedFetchResult.Failure(FeedResultStatus.ParseError, $"Invalid response JSON: {ex.Message}", body);
        }
    }

    private static JsonElement? ResolvePath(JsonElement root, string path)
    {
        var current = root;
        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(segment, out var next))
                return null;
            current = next;
        }

        return current;
    }

    private static decimal? ExtractDecimal(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Number when element.TryGetDecimal(out var number) => number,
        JsonValueKind.String when decimal.TryParse(element.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) => parsed,
        _ => null
    };
}
