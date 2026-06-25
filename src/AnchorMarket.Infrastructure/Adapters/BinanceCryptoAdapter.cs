using System.Globalization;
using System.Text.Json;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using AnchorMarket.Domain.Enums;

namespace AnchorMarket.Infrastructure.Adapters;

/// <summary>Fetches a spot price from Binance's REST ticker endpoint.</summary>
/// <remarks>
/// Config shape: <c>{ "Symbol": "BTCUSDT" }</c>. <see cref="ExternalFeedRegistration.ApiUrl"/> may override
/// the default endpoint; <see cref="ExternalFeedRegistration.TimeoutMs"/> bounds the request.
/// </remarks>
public class BinanceCryptoAdapter(IHttpClientFactory httpClientFactory) : IExternalFeedAdapter
{
    private const string DefaultEndpoint = "https://api.binance.com/api/v3/ticker/price";

    /// <inheritdoc />
    public string AdapterType => "BinanceCrypto";

    /// <inheritdoc />
    public async Task<FeedFetchResult> FetchAsync(ExternalFeedRegistration registration, CancellationToken cancellationToken)
    {
        string? symbol;
        try
        {
            using var configDoc = JsonDocument.Parse(string.IsNullOrWhiteSpace(registration.Config) ? "{}" : registration.Config);
            symbol = configDoc.RootElement.TryGetProperty("Symbol", out var symbolElement)
                ? symbolElement.GetString()
                : null;
        }
        catch (JsonException ex)
        {
            return FeedFetchResult.Failure(FeedResultStatus.ParseError, $"Invalid feed config JSON: {ex.Message}");
        }

        if (string.IsNullOrWhiteSpace(symbol))
            return FeedFetchResult.Failure(FeedResultStatus.ParseError, "Feed config must contain a non-empty 'Symbol'.");

        var endpoint = string.IsNullOrWhiteSpace(registration.ApiUrl) ? DefaultEndpoint : registration.ApiUrl;
        var requestUri = $"{endpoint}?symbol={Uri.EscapeDataString(symbol)}";

        var client = httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromMilliseconds(registration.TimeoutMs <= 0 ? 3000 : registration.TimeoutMs);

        string body;
        try
        {
            using var response = await client.GetAsync(requestUri, cancellationToken);
            body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
                return FeedFetchResult.Failure(FeedResultStatus.Failed, $"Binance returned HTTP {(int)response.StatusCode}.", body);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return FeedFetchResult.Failure(FeedResultStatus.Timeout, "Binance request timed out.");
        }
        catch (HttpRequestException ex)
        {
            return FeedFetchResult.Failure(FeedResultStatus.Failed, ex.Message);
        }

        try
        {
            using var responseDoc = JsonDocument.Parse(body);
            if (!responseDoc.RootElement.TryGetProperty("price", out var priceElement))
                return FeedFetchResult.Failure(FeedResultStatus.ParseError, "Response did not contain a 'price' field.", body);

            var priceText = priceElement.GetString();
            if (decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                return FeedFetchResult.Ok(body, price);

            return FeedFetchResult.Failure(FeedResultStatus.ParseError, $"Could not parse price '{priceText}'.", body);
        }
        catch (JsonException ex)
        {
            return FeedFetchResult.Failure(FeedResultStatus.ParseError, $"Invalid response JSON: {ex.Message}", body);
        }
    }
}
