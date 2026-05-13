namespace AnchorMarket.Application.Features.Groups.DTOs;

public record GroupDto(
    Guid Id,
    string Name,
    string? Description,
    Guid OwnerId,
    DateTimeOffset CreatedAt);
