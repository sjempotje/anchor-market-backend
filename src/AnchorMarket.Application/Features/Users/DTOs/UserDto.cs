namespace AnchorMarket.Application.Features.Users.DTOs;

public record UserDto(
    Guid Id,
    string? Username,
    string Name,
    string Email,
    bool EmailVerified,
    string? Image,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
