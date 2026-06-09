namespace AnchorMarket.Application.Features.Accounts.DTOs;

public record AccountDto(
    Guid Id,
    Guid UserId,
    string AccountId,
    string ProviderId,
    string? AccessToken,
    string? RefreshToken,
    DateTimeOffset? AccessTokenExpiresAt,
    DateTimeOffset? RefreshTokenExpiresAt,
    string? Scope,
    string? IdToken,
    string? Password,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
