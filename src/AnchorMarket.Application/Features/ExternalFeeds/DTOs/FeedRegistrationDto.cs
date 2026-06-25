namespace AnchorMarket.Application.Features.ExternalFeeds.DTOs;

/// <summary>Data transfer object for an external feed registration.</summary>
public record FeedRegistrationDto(
    Guid Id,
    Guid MarketId,
    string AdapterType,
    string Config,
    int PollingIntervalMs,
    int TimeoutMs,
    string? ApiUrl,
    int ResolutionGranularitySeconds,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
