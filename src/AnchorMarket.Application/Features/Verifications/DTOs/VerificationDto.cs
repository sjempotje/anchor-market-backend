namespace AnchorMarket.Application.Features.Verifications.DTOs;

public record VerificationDto(
    Guid Id,
    string Identifier,
    string Value,
    DateTimeOffset ExpiresAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
