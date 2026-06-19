namespace AnchorMarket.Application.Features.Verifications.DTOs;

/// <summary>Data transfer object for a verification code.</summary>
public record VerificationDto(
    Guid Id,
    string Identifier,
    string Value,
    DateTimeOffset ExpiresAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
