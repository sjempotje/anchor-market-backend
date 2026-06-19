namespace AnchorMarket.Application.Features.Sessions.DTOs;

/// <summary>Data transfer object for a user session. Does not expose the raw token.</summary>
public record SessionDto(
    Guid Id,
    Guid UserId,
    DateTimeOffset ExpiresAt,
    string? IpAddress,
    string? UserAgent,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
