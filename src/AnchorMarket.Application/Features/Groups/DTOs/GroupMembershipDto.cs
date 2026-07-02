namespace AnchorMarket.Application.Features.Groups.DTOs;

/// <summary>Data transfer object for a group membership.</summary>
public record GroupMembershipDto(
    Guid Id,
    Guid GroupId,
    Guid UserId,
    DateTimeOffset JoinedAt);
