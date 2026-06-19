namespace AnchorMarket.Application.Features.Sessions.DTOs;

/// <summary>Data transfer object for a user session.</summary>
public record SessionDto(
    Guid Id,
    Guid UserId,
    string Token,
    DateTimeOffset ExpiresAt,
    string? IpAddress,
    string? UserAgent,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
