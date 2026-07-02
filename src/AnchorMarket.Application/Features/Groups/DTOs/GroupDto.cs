namespace AnchorMarket.Application.Features.Groups.DTOs;

/// <summary>Data transfer object for a group.</summary>
public record GroupDto(
    Guid Id,
    string Name,
    string? Description,
    Guid OwnerId,
    bool IsPrivate,
    string? JoinCode,
    DateTimeOffset CreatedAt);
