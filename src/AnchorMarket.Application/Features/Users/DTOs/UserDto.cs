namespace AnchorMarket.Application.Features.Users.DTOs;

/// <summary>Data transfer object for a user public profile.</summary>
public record UserDto(
    Guid Id,
    string? Username,
    string Name,
    string? Image,
    string? Bio,
    bool IsVerifiedCreator,
    int FollowersCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
