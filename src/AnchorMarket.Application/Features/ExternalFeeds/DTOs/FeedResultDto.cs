using AnchorMarket.Domain.Enums;

namespace AnchorMarket.Application.Features.ExternalFeeds.DTOs;

/// <summary>Data transfer object for a captured feed result.</summary>
public record FeedResultDto(
    Guid Id,
    Guid FeedRegistrationId,
    string RawJson,
    decimal? ParsedValue,
    FeedResultStatus Status,
    string? ErrorMessage,
    DateTimeOffset ReceivedAt);
