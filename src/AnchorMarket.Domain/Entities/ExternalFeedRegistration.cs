namespace AnchorMarket.Domain.Entities;

/// <summary>Per-market configuration describing how an external data source is polled and parsed.</summary>
public class ExternalFeedRegistration : BaseEntity
{
    /// <summary>Gets the ID of the market this feed supplies data for.</summary>
    public Guid MarketId { get; private set; }

    /// <summary>Gets the adapter type that knows how to fetch and parse this feed (e.g. "BinanceCrypto", "CustomHttp").</summary>
    public string AdapterType { get; private set; } = string.Empty;

    /// <summary>Gets the adapter-specific configuration, stored as a JSON document.</summary>
    public string Config { get; private set; } = "{}";

    /// <summary>Gets how often the feed should be polled, in milliseconds.</summary>
    public int PollingIntervalMs { get; private set; }

    /// <summary>Gets the per-request timeout, in milliseconds.</summary>
    public int TimeoutMs { get; private set; }

    /// <summary>Gets the base API URL for the feed, when the adapter does not have a built-in default.</summary>
    public string? ApiUrl { get; private set; }

    /// <summary>Gets the optional bearer token sent with feed requests.</summary>
    public string? AuthToken { get; private set; }

    /// <summary>Gets the granularity, in seconds, used to downsample price history after the market resolves.</summary>
    public int ResolutionGranularitySeconds { get; private set; }

    /// <summary>Gets a value indicating whether the feed is currently being polled.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>Gets the market this feed supplies data for.</summary>
    public Market Market { get; private set; } = null!;

    /// <summary>Gets the raw results captured from this feed.</summary>
    public ICollection<FeedResult> Results { get; private set; } = new List<FeedResult>();

    /// <summary>Creates a new external feed registration for a market.</summary>
    /// <param name="marketId">The market this feed supplies data for.</param>
    /// <param name="adapterType">The adapter type handling this feed.</param>
    /// <param name="config">Adapter-specific configuration as JSON.</param>
    /// <param name="pollingIntervalMs">How often to poll, in milliseconds.</param>
    /// <param name="timeoutMs">Per-request timeout, in milliseconds.</param>
    /// <param name="apiUrl">Optional base API URL.</param>
    /// <param name="authToken">Optional bearer token.</param>
    /// <param name="resolutionGranularitySeconds">Post-resolution downsampling granularity, in seconds.</param>
    /// <returns>A new <see cref="ExternalFeedRegistration"/> instance.</returns>
    public static ExternalFeedRegistration Create(
        Guid marketId,
        string adapterType,
        string config,
        int pollingIntervalMs,
        int timeoutMs,
        string? apiUrl,
        string? authToken,
        int resolutionGranularitySeconds)
    {
        return new ExternalFeedRegistration
        {
            MarketId = marketId,
            AdapterType = adapterType,
            Config = string.IsNullOrWhiteSpace(config) ? "{}" : config,
            PollingIntervalMs = pollingIntervalMs,
            TimeoutMs = timeoutMs,
            ApiUrl = apiUrl,
            AuthToken = authToken,
            ResolutionGranularitySeconds = resolutionGranularitySeconds
        };
    }

    /// <summary>Updates the feed's configuration.</summary>
    public void Update(
        string config,
        int pollingIntervalMs,
        int timeoutMs,
        string? apiUrl,
        string? authToken,
        int resolutionGranularitySeconds)
    {
        Config = string.IsNullOrWhiteSpace(config) ? "{}" : config;
        PollingIntervalMs = pollingIntervalMs;
        TimeoutMs = timeoutMs;
        ApiUrl = apiUrl;
        AuthToken = authToken;
        ResolutionGranularitySeconds = resolutionGranularitySeconds;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Activates or deactivates polling for this feed.</summary>
    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
