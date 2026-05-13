namespace AnchorMarket.Application.Features.Users.DTOs;

public record UserDto(
    Guid Id,
    string Username,
    string Email,
    DateTimeOffset CreatedAt);
